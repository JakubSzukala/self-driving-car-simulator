from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from stable_baselines3 import PPO
from stable_baselines3.common.callbacks import EvalCallback
from stable_baselines3.common.callbacks import EveryNTimesteps

unity_env = UnityEnvironment(file_name=None, seed=1, side_channels=[])
env = UnityToGymWrapper(
    unity_env,
    False,
    False,
    False,
    1
)

model = PPO("MlpPolicy", env, verbose=1, tensorboard_log="tensorboard")
model.learn(total_timesteps=1_000)



unity_env.close()
