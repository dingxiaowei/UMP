#!/bin/sh
 
cd "$(dirname "$0")"
sudo apt-get remove libswscale-ffmpeg3 libavcodec-ffmpeg56
sudo apt-get autoremove
