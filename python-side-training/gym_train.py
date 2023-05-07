from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from stable_baselines3 import PPO
#from stable_baselines3.common.callbacks import 

unity_env = UnityEnvironment(file_name=None, seed=1, side_channels=[])
env = UnityToGymWrapper(
    unity_env,
    False,
    False,
    False,
    1
)

model = PPO("MlpPolicy", env, verbose=1)
model.learn(total_timesteps=1_000)
# TODO: Add callbacks and saving on best model
unity_env.close()
