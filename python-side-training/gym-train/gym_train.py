from datetime import datetime
import math
import os
import argparse
import sys

from typing import Optional, Tuple, Type, Union, Callable

from mlagents_envs.envs.unity_gym_env import UnityToGymWrapper
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

from stable_baselines3 import A2C, PPO, DQN
from stable_baselines3.common.callbacks import EvalCallback
from stable_baselines3.common.callbacks import CheckpointCallback

# TODO: Move it to some utils
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


# CLI initialization
usage_message ="""Example usage:
- Evaluate trained PPO model: python3 gym_train.py --action eval --rl_algorithm PPO --model logs/PPO-MlpPolicy-08-05-2023-16-51-19_best/best_model.zip
- Train a new model: python3 gym_train.py --action train --rl_algorithm PPO
- Continue training of an existing model: python3 gym_train.py --action cont_train --rl_algorithm PPO --model logs/PPO-MlpPolicy-08-05-2023-16-51-19_best/best_model.zip
If --exec argument is not provided, You need to have Unity Editor open with the environment prepared and after launching this script, launch the simulation in Unity"""

parser = argparse.ArgumentParser(
    usage=usage_message,
    description="Script for interfacing between Python and Unity for ML-agents training for self-driving-cal-simulator environment"
)
parser.add_argument(
    "-a", "--action",
    action="store",
    choices=["train", "cont_train", "eval"],
    required=True,
    help="Choose whether train a new model, continue training from existing model or evaluate trained model"
)
parser.add_argument(
    "-e", "--exec",
    action="store",
    help="Path to environment exec file, if None then Unity Editor with open environment must be used"
)
parser.add_argument(
    "-t", "--time_scale",
    type=float_range(0, math.inf),
    default=1.0,
    help="Time scale of the simulation 2.0 will make simulation run 2x faster"
)
parser.add_argument(
    "-l", "--rl_algorithm",
    action="store",
    choices=["PPO", "A2C", "DQN"],
    required=True,
    help="Reinforcement algorithm name"
)
parser.add_argument(
    "-m", "--model",
    action="store",
    help="Path to model"
)
parser.add_argument(
    "-s", "--steps",
    action="store",
    default=20_000,
    type=int,
    help="Number of steps to train the agent"
)

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


def main():
    # Parse arguments
    args = parser.parse_args()
    if (args.action == "cont_train" or args.action =="eval") and args.model is None:
        raise argparse.ArgumentTypeError("When selecting action cont_train or eval, You must provide path to model")

    # Environment initialization
    env = prep_env(args.time_scale, args.exec)
    model = prep_model(args.rl_algorithm, args.action, env, args.model)

    # Train
    if args.action == "train" or args.action == "cont_train":
        filename_best, filename_checkpoint = prep_logfile_name(args.rl_algorithm)
        eval_callback, checkpoint_callback = prep_callbacks(env, filename_best, filename_checkpoint)
        model.learn(total_timesteps=args.steps, callback=[eval_callback, checkpoint_callback], progress_bar=True)

    # Evaluation
    else:
        obs, reward, done, info = env.step(env.action_space.sample())
        for i in range(10_000):
            if done:
                env.reset()
                break
            action, state = model.predict(obs, deterministic=True)
            obs, reward, done, info = env.step(action)

    env.close()

main()

#model = PPO.load("logs/PPO-MlpPolicy-08-05-2023-11-24-54_best/best_model.zip")
#model = PPO.load("logs/PPO-MlpPolicy-08-05-2023-16-51-19_best/best_model.zip")