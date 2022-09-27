# Unity2020_MorphEnvironments

Unity code for Mark's morph environments, runnning training track, and Mark & Mari's hidden reward zone environments

## Branch notes

`master` -- stable code on NLW rig, updated after completion of GRABDA2m experiments

`dev` -- development branch of master for MetaLearn tracks on NLW rig

`cap_breakout` -- modified code (particularly arduino code) for use of capacitve breakout boards,  \
initially designed for Thorlabs rig and implemented on boltVR training rig
- recently modified such that rotary encoder is commented out of the arduino code
- now unstable

`cap_single_arduino` -- stable copy of `cap_breakout` that Mari made before messing with `cap_breakout`  \
on the Thorlabs rig (TO-DO: make sure rotary lines are not commented out!)

`boltvr_single_arduino` -- stable single arduino verion on the boltVR rig, originally from `cap_breakout`  \
but modified with Linux ports and paths

`thorlabs_dual_arduino` -- code for two-arduino (separate lickport and rotary) system on the Thorlabs rig,  \
with a working Running Training task with no bugs
