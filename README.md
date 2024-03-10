#for build project:
dotnet publish -c Release -o published

#for run without docker
dotnet published/InstagramPars.dll



#for docker container:
#build:
docker build -t glider4d/instaparser .

#run  interactive mode:
docker run -it --rm -p 5000:80 --name s_instagram glider4d/instaparser


#run in background mode:

docker run -d -p 5000:80 --name s_instagram glider4d/instaparser