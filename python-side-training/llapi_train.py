import numpy as np
from mlagents_envs.environment import UnityEnvironment

#env = UnityEnvironment(file_name="../environment.x86_64", seed=1, side_channels=[])
env = UnityEnvironment(file_name=None, seed=1, side_channels=[])
env.reset()

behavior_name = list(env.behavior_specs.keys())[0]
print(behavior_name)

# Readonly environment / behaviour informations
behavior_spec = env.behavior_specs[behavior_name]
print(behavior_spec)

# Information about agent that needs action or ended an episode (?)
# [0] DecisionSteps - obs, acts, rews, agent id, action mask, this agents
# need action this step (this is batch, not single agent)
# [1] TerminalSteps - the same but for agents that end episode
steps = env.get_steps(behavior_name)
for step in steps:
    print(step.obs)

for i in range(100):
    #print("Step: ", i)
    steps = env.get_steps(behavior_name)
    for step in steps:
        print(np.unique(step.obs))
    action = behavior_spec.action_spec.random_action(1)
    env.set_actions(behavior_name, action)
    env.step()