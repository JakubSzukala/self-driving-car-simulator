import math
import argparse

from utilities import *

# CLI initialization
usage_message ="""

- Evaluate trained PPO model: python3 gym_interface.py --action eval --rl_algorithm PPO --model logs/PPO-MlpPolicy-08-05-2023-16-51-19_best/best_model.zip
- Train a new model: python3 gym_interface.py --action train --rl_algorithm PPO
- Continue training of an existing model: python3 gym_interface.py --action cont_train --rl_algorithm PPO --model logs/PPO-MlpPolicy-08-05-2023-16-51-19_best/best_model.zip
- In separate terminal run: tensorboard --logdir <tensorboard log directory> to open tensorboard dashboard
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
        filename_best, filename_checkpoint, tb_log_filename = prep_logfile_names(args.rl_algorithm)
        eval_callback, checkpoint_callback = prep_callbacks(env, filename_best, filename_checkpoint)
        model.learn(total_timesteps=args.steps, callback=[eval_callback, checkpoint_callback], progress_bar=True, tb_log_name=tb_log_filename)

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