#!/bin/sh

apt-get update
apt-get install -y libc6-dev libgdiplus libx11-dev

# this will exec CMD
exec "$@"