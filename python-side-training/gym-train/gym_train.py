from datetime import datetime
from os.path import join

from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from stable_baselines3 import PPO
from stable_baselines3.common.callbacks import EvalCallback
from stable_baselines3.common.callbacks import CheckpointCallback

engine_config_channel = EngineConfigurationChannel()
unity_env = UnityEnvironment(file_name=None, seed=1, side_channels=[engine_config_channel])
engine_config_channel.set_configuration_parameters(time_scale=2.0)

env = UnityToGymWrapper(
    unity_env,
    False,
    False,
    False,
    1
)

# TODO: Add CLI interface or parsing of some config yaml file

algorithm = "PPO"
policy = "MlpPolicy"

now = datetime.now()

date_string = now.strftime("%d-%m-%Y-%H-%M-%S")
filename_best = algorithm + "-" + policy + "-" + date_string + "_best"
filename_checkpoint = algorithm + "-" + policy + "-" + date_string + "checkpoint"

# Eval is in the same environment, track is randomized, no need to change
eval_callback = EvalCallback(env,
                             best_model_save_path=join("logs", filename_best),
                             log_path=join("logs", filename_best),
                             eval_freq=1000,
                             deterministic=True,
                             render=False)

checkpoint_callback = CheckpointCallback(
    save_freq=1000,
    save_path="logs",
    name_prefix=filename_checkpoint,
    verbose=2
)

# Instantiate and start learning loop
model = PPO("MlpPolicy", env, verbose=1, tensorboard_log="tensorboard")
model.learn(total_timesteps=45000, callback=[eval_callback, checkpoint_callback], progress_bar=True)

unity_env.close()
