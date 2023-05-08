from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment

from stable_baselines3 import PPO

unity_env = UnityEnvironment(file_name=None, seed=1, side_channels=[])

env = UnityToGymWrapper(
    unity_env,
    False,
    False,
    False,
    1
)

model = PPO.load("logs/PPO-MlpPolicy-08-05-2023-11-24-54_best/best_model.zip")

obs, reward, done, info = env.step(env.action_space.sample())
for i in range(10_000):
    action, state = model.predict(obs, deterministic=True)
    obs, reward, done, info = env.step(action)