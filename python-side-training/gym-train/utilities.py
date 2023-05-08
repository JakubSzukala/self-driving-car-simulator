import argparse
from datetime import datetime
import os

from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from stable_baselines3 import A2C, PPO, DQN
from stable_baselines3.common.callbacks import EvalCallback
from stable_baselines3.common.callbacks import CheckpointCallback

from typing import Optional, Tuple, Type, Union, Callable

def float_range(min: float, max: float) -> callable:
    def float_range_checker(arg):
        try:
            f = float(arg)
        except:
            raise argparse.ArgumentTypeError("Argument must be a floating point number")
        if f < min or f > max:
            raise argparse.ArgumentTypeError("Argument must be in range {min} - {max}".format(min, max))
        return f
    return float_range_checker


def prep_env(time_scale: float, exec_filename: str) -> UnityToGymWrapper:
    engine_config_channel = EngineConfigurationChannel()
    unity_env = UnityEnvironment(file_name=exec_filename, side_channels=[engine_config_channel])
    engine_config_channel.set_configuration_parameters(time_scale=time_scale)
    env = UnityToGymWrapper(unity_env, False, False, False, 1)
    return env


def prep_train_model(rl_algorithm: str, env: UnityToGymWrapper) -> Union[PPO, A2C, DQN]:
    if rl_algorithm == "PPO":
        new_model = PPO("MlpPolicy", env, verbose=1, tensorboard_log="tensorboard")
    elif rl_algorithm == "A2C":
        new_model = A2C("MlpPolicy", env, verbose=1, tensorboard_log="tensorboard")
    elif rl_algorithm == "DQN":
        new_model = DQN("MlpPolicy", env, verbose=1, tensorboard_log="tensorboard")
    return new_model


def prep_eval_model(rl_algorithm: str, model_path: str):
    if rl_algorithm == "PPO":
        new_model = PPO.load(model_path)
    elif rl_algorithm == "A2C":
        new_model = A2C.load(model_path)
    elif rl_algorithm == "DQN":
        new_model = DQN.load(model_path)
    return new_model


def prep_cont_train_model(rl_algorithm: str, env: UnityToGymWrapper, model_path: Optional[str]) -> Union[PPO, A2C, DQN]:
    if rl_algorithm == "PPO":
        new_model = PPO.load(model_path)
    elif rl_algorithm == "A2C":
        new_model = A2C.load(model_path)
    elif rl_algorithm == "DQN":
        new_model = DQN.load(model_path)
    new_model.set_env(env)
    return new_model


def prep_model(rl_algorithm: str, action: str, env: UnityToGymWrapper, model_path: Optional[str]) -> Union[PPO, A2C, DQN]:
    if action == "train":
        new_model = prep_train_model(rl_algorithm, env)
    elif action == "eval":
        new_model = prep_eval_model(rl_algorithm, model_path)
    elif action == "cont_train":
        new_model = prep_cont_train_model(rl_algorithm, env, model_path)
    return new_model


def prep_logfile_name(rl_algorithm: str) -> Tuple[str, str]:
    now = datetime.now()
    date_string = now.strftime("%d-%m-%Y-%H-%M-%S")
    filename_best = rl_algorithm + "-" + "MlpPolicy" + "-" + date_string + "_best"
    filename_checkpoint = rl_algorithm + "-" + "MlpPolicy" + "-" + date_string + "checkpoint"
    return filename_best, filename_checkpoint


def prep_callbacks(env: UnityToGymWrapper, filename_best: str, filename_checkpoint: str) -> Tuple[EvalCallback, CheckpointCallback]:
        eval_callback = EvalCallback(
            env,
            best_model_save_path=os.path.join("logs", filename_best),
            log_path=os.path.join("logs", filename_best),
            eval_freq=1000,
            deterministic=True,
            render=False
        )
        checkpoint_callback = CheckpointCallback(
            save_freq=2000,
            save_path="logs",
            name_prefix=filename_checkpoint,
            verbose=2
        )
        return eval_callback, checkpoint_callback

