# Autonomous cars project

Author: Jakub Szukała

The project is a submission for the Autonomous Cars subject taught at PUT.

## Solution description

Among various approaches to the problem, I decided to go with neural network with reinforcement learning. This neural network will be given vector of observations from lidar laser scanner, make the decision about acceleration and wheels turn angle and this information will be then given to the car to execute it.

Source code for the project is split into two:
- ML-agents Unity simulation - [source](https://github.com/JakubSzukala/self-driving-car-simulator)
- ROS2 package performing inference from subscribed lidar data. In the package is also model that achieved the best results - [source](https://github.com/JakubSzukala/nn-controller)

#### Methodology
The training environment runs in Unity with [ML-agents](https://github.com/Unity-Technologies/ml-agents), which is Unity's package that facilitates machine learning in the game engine. It also has a RGLUnityPlugin (extracted directly from AWSIM) that contains exactly the same lidar sensing as the test environment. Normally in ML-agents, the model would be trained inside Unity and then saved as ONNX file. This approach is not applicable here, as the ONNX file is not really exportable outside of Unity (no idea why but people had issues with that), so I used alternative solution. Idea is to just use Unity as an environment for [Gym](https://gymnasium.farama.org/) (otherwise now known as Gymnasium) and all the machine learning was performed with Stable-Baselines3, which is an rl-overlay for PyTorch. Experimentatlly I checked that by far best results are obtained with PPO architecture. Such trained model will then be exported and inserted into a ROS2 node that will subscribe to lidar sensor data and publish accelerations and turn angles.

#### Reinforcement learning - training setup
[![training video](https://img.youtube.com/vi/0ZzvdyZjbs4/0.jpg)](https://youtu.be/0ZzvdyZjbs4)

[Video](https://youtu.be/0ZzvdyZjbs4)

###### Envrionment and rewards description
I prepared reduced simulation envrionment. The simulation was created from ground up, only parts that were reused were car model and RGL library. The car model is exactly the same as it is in the test environment (I exported it as unity package from AWSIM) but I removed all references to ROS. During training there is **new randomly generated race track every training iteration**. The car is spawned at the beginning of the track and it has to drive through the track to accumulate a reward. Reward function is as follows:
- for each second receive a negative reward of -0.1
- for each checkpoint on the track get 1500/N points, where N is number of checkpoints in the track (agent will more easily understand to drive forward)
- for driving of the track get a reward of -100 (agent has to drive safely)
- for finishing the entire track get +1500
- there is minimum score threshold (-150) on which epoch will be stopped, as the agent clearly is not gaining any reward

Reward function was inspired by [CarRacing](https://gymnasium.farama.org/environments/box2d/car_racing/) from gymnasium.

###### Randomly generated race track description
Benefits of randomly generated race track are obvious, as agent will generalize way better, if it has encountered many different scenarios, which obviously is very important in control task. So random race track generation is done with few steps:
- generate random points on XZ plane
- use [gift wrapping](https://en.wikipedia.org/wiki/Gift_wrapping_algorithm) algorihtm to create a closed loop / path
- this path is convex by definition, so displace randomly points to make it concave (we do not want a car to learn only to turn right or left)
- create smooth path from very rough path using Bezier Curves, this way we increase number of points and create nice smooth curves
- by applying bezier curves we increase number of points and in process smooth out textures of road and walls, which makes them more similar to test environment and may be relevant during training (more realistic lidar observations, especially on tight turns)
- applying Bezier also solved a problem with large amount of artifacts that occured with also tried [Chaikin smoothing](https://sighack.com/post/chaikin-curves), for example, walls would very often overlap / cross each other on curves
- road has a mesh collider but walls do not, they are still visible for lidar scanner but the car can fall of the track
- this way of generating a track is not perfect, so there is also added algorithm that takes traingles from the road mesh, and checks if there are any collisions / overlaps, if there are, generate new race track and do that until correct track is obtained

###### Training on Unity side
ML-agents has very neat, concise and convinient API for implementing agents. It basically wasa necessary to override three functions in Agent base class:
- OnEpisodeBegin - sets up the environment on the beginning of each epoch. Resets agent, randomizes track etc.
```csharp
public override void OnEpisodeBegin()
{
    // Spawn race track
    raceTrack.CreateRaceTrack(true);

    // Set the agent car at the start
    Vector3 start, direction;
    raceTrack.GetRaceTrackStart(out start, out direction);

    // Adjust reward for checkpoint, so longer track will award the same amount as short track
    checkpointReward = trackFinishedReward / raceTrack.checkPointContainer.transform.childCount;

    // Randomly change driving direction
    float seed = Random.Range(0f, 1f);
    if (seed > 0.5f) direction = -direction;

    // Instantiate a car, get reference to it and assign lidar subscriber
    GameObject.Destroy(agentCar);
    agentCar = carSpawner.spawnCar(start, direction);
    lidarDataSubscriber = new LidarDataSubscriber(agentCar.GetComponentInChildren<LidarSensor>());

    // Start timers
    penaltyTimer.StartTimer();
    episodeTimer.StartTimer();
}
```

- CollectObservations - here we provide observations from lidar to the agent.
```csharp
public override void CollectObservations(VectorSensor sensor)
{
    float[] normalizedDistances = new float[lidarDataSubscriber.Distances.Length];
    for (int i = 0; i < lidarDataSubscriber.Distances.Length; i++)
    {
        normalizedDistances[i] = lidarDataSubscriber.Distances[i] / agentCar.GetComponentInChildren<LidarSensor>().configuration.maxRange;
    }
    sensor.AddObservation(normalizedDistances);
}
```

- OnActionReceived - here we apply actions that were infered on the network and check the state of the environment, whether I should finish the episode. Also a reward is calculated here.
```csharp
public override void OnActionReceived(ActionBuffers actionBuffers)
{
    // Apply actions
    float controlSignalAcc, controlSignalSteer;
    controlSignalAcc = maxAcceleration * actionBuffers.ContinuousActions[0];
    controlSignalSteer = maxSteerAngle * actionBuffers.ContinuousActions[1];
    if (limitSteeringAngleChange)
    {
        float steeringAngleChange = controlSignalSteer - agentCar.GetComponent<Vehicle>().SteerAngleInput;
        float clampedSteeringAngleChange = Mathf.Clamp(steeringAngleChange, -maxSteeringAngleChange, maxSteeringAngleChange);
        Debug.Log("Before: " + agentCar.GetComponent<Vehicle>().SteerAngleInput + " and after " + (agentCar.GetComponent<Vehicle>().SteerAngleInput + clampedSteeringAngleChange) + " and change: " + steeringAngleChange);
        agentCar.GetComponent<Vehicle>().SteerAngleInput = agentCar.GetComponent<Vehicle>().SteerAngleInput + clampedSteeringAngleChange;
    }
    else
    {
        agentCar.GetComponent<Vehicle>().SteerAngleInput = controlSignalSteer;
    }
    agentCar.GetComponent<Vehicle>().AccelerationInput = controlSignalAcc;

    (...)
}
```

Entire source file can be reviewed [here](https://github.com/JakubSzukala/self-driving-car-simulator/blob/master/Assets/RaceCarAgent.cs).

These functions will be "handles" for the training loop implemented in python with Stable-Baselines3.

###### Training Python side
Before training though, we have to prepare a gym environment. Thankfully, Unity provides a wrapper for that, that will use under the hood exact functions described above.
Training is straight forward (prep_logfile_names and prep_callbacks are abstractions created by us):
```python
filename_best, filename_checkpoint, tb_log_filename = prep_logfile_names(args.rl_algorithm)
eval_callback, checkpoint_callback = prep_callbacks(env, filename_best, filename_checkpoint)

# PPO
model.learn(total_timesteps=args.steps, callback=[eval_callback, checkpoint_callback], progress_bar=True, tb_log_name=tb_log_filename)
```

Under the hood I also implemented callbacks that save the best performing model during the evaluation (every 5000 steps) step and periodically save the model (every 2000 steps).

After training we can perform the evaluation with the same script, providing different arguments. Evaluation is performed in the same Unity environment but with new randomized race track. Fragment of code used for evalutation:
```python
# In unity env observation is already normalized
obs, reward, done, info = env.step(env.action_space.sample())
lstm_states = None
episode_start = True
for i in range(10_000):
    if done:
        env.reset()
        break
    action, lstm_states = model.predict(
        obs,
        state=lstm_states,
        episode_start=episode_start,
        deterministic=True
    )
    obs, reward, done, info = env.step(action)
    episode_start = False
```
See the full source file [here](https://github.com/JakubSzukala/self-driving-car-simulator/blob/master/python-side-training/gym-train/gym_interface.py) and utilities file [here](https://github.com/JakubSzukala/self-driving-car-simulator/blob/master/python-side-training/gym-train/utilities.py).

#### Implementing inference in ROS2
[![testing video](https://img.youtube.com/vi/nq-WuxvkT6M/0.jpg)](https://youtu.be/nq-WuxvkT6M)

[Video](https://youtu.be/nq-WuxvkT6M)

With most of the heavy lifting done by a neural network, the code in ROS2 is quite straight forward. First, we have to instantiate and initialize a model. We obviously do not want to prepare separate gym environment for that, so we will only extract policy from the PPO model. Policy will allow us to perform inference:
```python
self.model_path = 'models/best_model.zip'
model = PPO.load(self.model_path)
self.policy = model.policy
del model
```

Then we create a publisher and subscriber for control signals and laser scans respectively. In the callback for subscriber we implement inference:
```python
def control_callback(self, msg: LaserScan):
    self.get_logger().info("Received laser scan:")
    observations = np.array(msg.ranges)
    observations = observations / msg.range_max # Normalize
    # Filter infs and -infs
    mask = np.isin(observations, np.inf)
    observations[mask] = msg.range_max
    mask = np.isin(observations, [-np.inf, np.nan])
    observations[mask] = 0

    observations = observations[::-1]
    # Predict
    action = self.policy.predict(observations, deterministic=True)

    console_log = "Actions taken: {}, {}".format(action[0][0], action[0][1])
    self.get_logger().info(console_log)

    # Act
    control_msg = AckermannControlCommand()
    control_msg.longitudinal.acceleration = float(action[0][0])
    control_msg.lateral.steering_tire_angle = float(action[0][1])

    self.publisher.publish(control_msg)
```
See full source file [here](https://github.com/JakubSzukala/nn-controller/blob/master/nn_controller/nn_controller_node.py).

Funnily enough, the observations that are delivered into ROS are in opposite order than they are in Stable-Baselines3, which caused a few hours confusion on why the car does not want to drive.

#### Results and conclusions
Resutls seem to be quite satisfactory, as the car is able to drive pretty well:

insert gif here

###### Jittering
Car is pretty jittery when it comes to turning. This is due a fact that there is no reason for the agent not to do that, as the reward function does not punish such behaviour and there is no limitation between setting min -> max in two consecutive steps. This exact issue is discussed in [this blog](https://towardsdatascience.com/learning-to-drive-smoothly-in-minutes-450a7cdb35f4?fbclid=IwAR25lyWXdvCGCw44dQAvtFi_umCuHRfAXRLCwBT_eHNQ6bz-HjQEAp6dGvw). I implemented a limitation to the angle change, which improved jittering, but it should be treated as tuning a hyper parameter: if it is too large, the car will be jittery, if it is too small, the car will not be able to make through sharp corners. In our current results there is improvement visible but probably better results could be reached by further decreasing the limitation. I also noted that with increased learning horizon, the jittering is getting worse. Below is presented a fragment of code responsible for clamping, it is implemented in OnActionReceived function of an agent:

```csharp
if (limitSteeringAngleChange)
{
    float steeringAngleChange = controlSignalSteer - agentCar.GetComponent<Vehicle>().SteerAngleInput;
    float clampedSteeringAngleChange = Mathf.Clamp(steeringAngleChange, -maxSteeringAngleChange, maxSteeringAngleChange);
    Debug.Log("Before: " + agentCar.GetComponent<Vehicle>().SteerAngleInput + " and after " + (agentCar.GetComponent<Vehicle>().SteerAngleInput + clampedSteeringAngleChange) + " and change: " + steeringAngleChange);
    agentCar.GetComponent<Vehicle>().SteerAngleInput = agentCar.GetComponent<Vehicle>().SteerAngleInput + clampedSteeringAngleChange;
}
else
{
    agentCar.GetComponent<Vehicle>().SteerAngleInput = controlSignalSteer;
}
agentCar.GetComponent<Vehicle>().AccelerationInput = controlSignalAcc;
```

###### Performance issues
The simulation can sometimes slow down on Nvidia GTX1050, which causes a delay and sometimes throws the car onto the wall. To improve reliability we can decrease time scale. We also tried to move the model inference to CPU to offload some work from GPU, but it does not seem to impact the performance.

###### Differences between training and test simulations
The walls in test simulation are pretty low and the car has modelled dampers, which cause a lidar scan to go above the walls (especially when car jitters a lot). There was an attempt to model that in training simulation but the car seems to be more damped than in test simulation.

###### Race track shape
In training simulation, race track is smoothed with Bezier Curves which will not allow very sharp turns, that do actually occur in test simulation. To improve results we could try other solution than Bezier Curves or in the current solution we could try other placement of control points in each pair of anchor points (see [here](https://youtu.be/nNmFLWup4_k?t=426))

###### Potential introdcutions to the learning loop
Also, as discussed with supervisor, we could maybe filter out the lidar scans that are not relevant. There could be also added PID controller to the controll loop

###### RecurrentPPO
There was an attemp to train RecurrentPPO (PPO with memory), which apparently improves jittering, but I could not train it, the car would only drive in one direction, so it is something that could be explored.

###### Improving reward function
Reward function could also be improved as during evalutation we are **not sure** that reward function will always indicate the best model. The reward is scaled so the car will get the same reward after completing the track no matter its length and complexity. But the issue is that when during evalutation agent will be given 5 easier tracks it will more consistently gain better reward than when it gets 5 complex and hard tracks.

###### RGL library issues
While using RGL library I encountered, seemingly randomly, issue with crashing the library and with it an entire Unity editor. It happens sporadically but is quite an issue when training the model for long periods of time. Fortunately I had model - saving callback set up, so nothing was lost. The error did not seem to come from some illegal reference or null exception. It would be hard to debug as the RGL is C++ library loaded to Unity as dll. It may also be hardware issue, which will not be reproducible on other machines.

###### Final conclusion
By far the most work was put into the training environment, so there is a lot of room for improvement when it comes to tuning the model, but overall the project was very interesting learning experience and I am happy with the results.
