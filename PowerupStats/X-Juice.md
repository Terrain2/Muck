This powerup has not been implemented in the game yet. It does have a calculation function, which is hardcoded to just return 1 at the moment. That function is not used for anything in the game, so i have no idea what that multiplier is used for, however when a critical hit occurs, a function called `StartJuice()` is called, which is empty, but its equivalent `StopJuice()` resets the "juice speed" to 1, and that juice speed is used to multiply the attack speed, and presumably other things in the future.