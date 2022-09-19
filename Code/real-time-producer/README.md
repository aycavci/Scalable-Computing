# Real Time Producer
This folder contains the code for a real time data producer. It gets data from
the database and puts in on Kafka.


All integeration settings should be set correctly in `appsettings.json`. For
development purposes the values in `appsetings.Development.json` can be used.
Just move those contents to `appsetings.json`.

## Run via docker
The application can be build using the Dockerfile and then run. Be aware to set
the right values in `appsetings.json`
```
docker build -t real-time-producer:latest .
docker start -d --name real-time-producer real-time-producer
```


## Pushing application to docker hub
```
docker logout
docker login
docker tag real-time-producer:latest sytseoegema/rug-sc:latest
docker push sytseoegema/rug-sc
```
