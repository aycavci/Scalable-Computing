# master
This folder contains the code for a master that trains models. After the
training phase the best model is used for real time predictions.

<!--
All integeration settings should be set correctly in `appsettings.json`. For
development purposes the values in `appsetings.Development.json` can be used.
Just move those contents to `appsetings.json`. -->

## Run via docker
The application can be build using the Dockerfile and then run.
```
docker build -t master:latest .
docker run -p 8080:8080 -d --name master master main.py
```


## Pushing application to docker hub
```
docker logout
docker login
docker tag master:latest sytseoegema/rug-sc-master:latest
docker push sytseoegema/rug-sc-master
```
