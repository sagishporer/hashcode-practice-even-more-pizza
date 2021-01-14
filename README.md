# Google Hash Code 2021 practice problem solution: Even More Pizza

[Problem statement](https://bytefreaks.net/google/hash-code/google-hash-code-2021-practice-problem)

## Algorithm description:
* Phase 1: Build a solution 
* Phase 2: Optimizations 

### Phase 1 - Build a solution
1. Sort pizzas by number of ingredients.
2. Build deliveries, first teams of 4, after that 3, after that 2:
   2.1 Select the pizza with the most ingredients.
   2.2 Select the pizza that will give the best improvement in delivery (most new ingredients, with the least overlaping ingredients).
   2.3 Repeat 2.2 until the delivery is ready.

### Phase 2 - Optimization
1. Try to swap 2 pizzas between any 2 deliveries - if it improves the score, make the swap.
2. Try to swap 1 pizza between any 2 deliveries - if it improves the score, make the swap.
3. Try to swap a pizza from any delivery with unused pizza - if it improves the score, make the swap.
4. Try to move 1 pizza between 2 deliveries (# of pizza in the 2 deliveries must be -+1) - if it improves the score, make the swap.
5. If any improvement performed in 1-4 - go to 1

### Notes
1. Phase 1 takes about 5 seconds to run.
2. Phase 2 takes about 50 minutes to run with the current restrictions (implemented for D & E which are huge). About 1% score improvement.

## Scores
Score: 731,455,475 

* A – Example - 74 points (74 before optimization)
* B – A little bit of everything - 13,533 points (12922 before optimization) 
* C – Many ingredients 712,692,751 points (706,624,572 before optimization)
* D – Many pizzas - 7,911,296 points (7,863,102 before optimization)
* E – Many teams - 10,837,821 points (10,789,627 before optimization)
