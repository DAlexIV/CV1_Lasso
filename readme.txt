//Made by Ivanov Alex 2015 as Homework for CV seminar
Q: How to use it?
A: Click on the Magic button, then click on the right picture to set start point. After some time, while
preprocessing goes you can take a break. When preprocessing is over, you can draw, simply by holding left
mouse button and moving it somewhere close to segment border. As you realise mouse button, your way will 
be shown.
Q: Why it works so slow?
A: I really don't know. Djikstra executes with priority queue, so it should have n*(log n) complexity. When 
the image size is less than 100 Kbytes, it works pretty fast, even on my notebook. But if we want to process
FullHD image it takes a few minutes. If I use more powerful processor(i7-3770k @ 4.5 Ghz), it boosts by 2 times. 
But it doesn't matter, because it definitely not real-time processing. It's clearly seen, that if we want to make
a huge boost, we need to parallelize on many threads or compress image as well.
