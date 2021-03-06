## Cumulative Distribution Information

This isn't a powerup, but an explanation of the math behind how they scale. If you don't care about how exactly it works or wanna just see the pictures of the results, scroll down.

A lot of powerups get stronger over time, but eventually cap out, no matter how many powerups you have. There's a function in math that tells you how likely a random value `n` is to be less than or equal to a specific value `x`. These can be plotted on a graph, giving you a lookup table so that if you look at the `x` axis for a specific value, the `y` axis tells you how likely the random value is to be below that number. `y` never quite reaches 100%, but it approaches that more and more as `x` gets infinitely big.

Some chance functions in the game use that function to actually get the probability you should have, and then getting a random value. If the random value is below the value it calculated, then an effect is applied (such as the snipe scope chance). However, Muck also uses the same function, scaled upwards, to determine how *strong* some powerups should be (such as the sniper scope actual damage multiplier). 

The function the game uses to calculate these values takes in two parameters: `a` (the 'scale speed') and `b` (the 'max value'), which are the same for the "same calculation" always (i.e. those parameters always are the same for every time it calculates the speed boost for the sneakers), and one parameter `x`, which is different for each calculation. It is always an integer (in the context of powerups, that's how many of the powerup you have, but some parts of the game do use the same function with a different value for this parameter). That function returns the calculation `b(1-e^(-ax))`, where `e` is Euler's constant. In powerup descriptions, i will not be writing the formula, just its parameters.

It's not gonna make much of a difference, but just for accuracy's sake, if you want the exact result the game produces, it's using single-precision (32-bit) floating point numbers and `e` has the exact value of `2.71828`. It uses Unity's built in `Mathf.Pow` function, which as far as i know uses instructions, so they are not completely consistent across platforms. If you want completely 100% for sure exactly identical results to the game, get fucked. You can't.

This function can be graphed using a tool like [desmos](https://www.desmos.com/calculator), by replacing `a` and `b` in the formula with the actual values, and setting that as the value for `y`, which is what i've used for all the graphs on this page. You can click on any graph i've made to view the interactive desmos page for it.

# YOU CAN CLICK ON ANY GRAPH AND YOU CAN ZOOM OUT AND SHIT AND SEE THE EQUATIONS ON THE LEFT SIDE