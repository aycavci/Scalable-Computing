# RUa* predictor
This repository contains the contents of the RUa* (say: Are You a Star) predictor.

**Project by Group 1**  
Ayça Avcı  
Sytse Oegema  
Merijn Schröder 

## Project Description
In this project, we train an XGBoost model for predicting the average IMDB rating of an actor.
In order to find the best parameters for the XGBoost model, we make use of parallel parameter optimization, which is the main focus of the project.
After a model is trained using the best parameter combination, predictions are made on streaming data.
This document describes the data being used, the actual implementation, and the major architectural decisions.
In the last section of this document we go over possible future steps.

## Technologies
We use multiple technologies and libraries for this project.
This section contains an alphabetically ordered list.
Some less fundamental Python libraries for this project are left out.
An extended explanation of each of them can be found in the corresponding section.

* [.NET](https://dotnet.microsoft.com/)
* [Apache Kafka](https://kafka.apache.org/)
* [Apache Avro](http://avro.apache.org/)
* [Apache Spark](https://spark.apache.org/) (PySpark)
* [Docker](https://www.docker.com/)
* [Kubernetes](https://kubernetes.io/)
* [PostgreSQL](https://www.postgresql.org/)
* [Python 3.7](https://www.python.org/)
* [SQLAlchemy](https://www.sqlalchemy.org/)
* [XGBoost Python Package](https://xgboost.readthedocs.io/en/release_1.3.0/python/python_intro.html)

## Requirements
In this section, we will go over the requirements for both developing and running the project.

### Project development requirements
The project in development mode can be run using the following software:
- **Postgres SQL**: either setup a docker container or install Postgres on the
host system.
- **Kafka (+ Zookeeper & SchemaRegistry)**: easiest way to setup is by using
the [Confluent packages](https://docs.confluent.io/platform/current/installation/installing_cp/overview.html#installation).
- **.NET SDK v 5.0**: required for the real time data producer. This can also
be run using Docker without installing .NET on your host system.
- **Python 3**

### Project production requirements
The project can be run on a Kubernetes production environment. Only Kubernetes
and [Helm](https://helm.sh/) are required in this case.

## Data
For this project, we make use of a dataset published on [Kaggle](https://www.kaggle.com/ashirwadsangwan/imdb-dataset) containing information about titles (movies, TV-shows, commercials, etc.) and actors retrieved.
This data is retrieved from online database [IMDB](https://www.imdb.com/).
The goal of this project is to predict the average rating of an actor.
This rating is the sum of the average ratings of all titles an actor starred in.
This means that even though a title has less ratings, the average rating of the title does contribute just as much to value as all other titles.
This decision is made in order to eliminate a hypothetical correlation between the popularity and the number of ratings.

The features to predict the average rating of an actor are mainly based on the information of the titles an actor starred in.
Examples are the percentage of an actor's titles with a certain genre, and the percentage of title types (e.g., movie, tv-show).
Additionally, the birth year of the actor and the number of titles the actor starred in are part of the feature list.

When downloading the dataset from Kaggle, there are multiple files which represent different tables.
All tables are inserted in the database.
Preprocessing the data is done in SQL and the results are stored in a separate table.
The reason for this is that each time the application starts, the data preprocessing step can be skipped and the data can be retrieved from the table containing the data in the right format.
It is important to note, however, that this is mainly useful for testing and developing.
When this project is uses actual real-time data, the preprocessing step of the data cannot be skipped.
Since the original tables are still in the database as well, it is always possible to access the raw data.

The largest part of the dataset is used for training the model.
The remaining part is used as streaming data.
How streaming data is implemented and processed will be discussed in a later section.

The data is stored in a PostgreSQL database.
As further explained in the next sections, the main application of this project is written in Python 3.7.
In order to load the the data from the database and store the results, we are using the Python package SQLAlchemy.

## Parallel Parameter Optimization
In this project, we are training an XGBoost model, which will make predictions on streaming data.
In order to find the best values to assign to the parameters of the model, we train and evaluate multiple models with different combinations of values for the parameters.
Finally, we pick the combination of parameter values which results the best performing model and train the final model using these parameters.

### Data Preparation
First, the dataset which we used for training the model is retrieved from the database.
This dataset is split into two parts: 75% of the dataset is used for parameter optimization and the training of the model, and the other 25% is used for evaluating the final model which will be used to make predictions.

### Parameter Optimization
Because there can be many different parameter combinations for which a model has to be trained, finding the best values for the parameters can be a very time consuming task.
To speed this process up, we parallelize the parameter optimization using [Apache Spark](https://spark.apache.org/) (PySpark).

First, an extra column is added to the dataset containing values for different parameters for training an XGBoost model.
For example, the set of parameter values _P_ is added to row _i_.
Next, all rows with the same set of parameter values are grouped together.
This means that row _i_ is placed in a group together with all other rows to which set _P_ was assigned.
Next, for each group of rows, a model is trained using the corresponding set of parameter values.

In the current implementation, the dataset is duplicated so that each row is assigned to each group.
I.e., if there are _n_ different parameter combinations to consider, the dataset is duplicated _n_ times.
The reason for this decision is to make it possible to scale up the number of parameter value combinations without any problems.
If the data was split over the different groups, eventually the part of the dataset in a group will become too small to get reliable results of the performance of the parameter values.

Now the groups are generated, a model is trained for each group in parallel.
For this, a User Defined Function (UDF) function is used.
This function splits the part of the dataset it receives for training and testing, and trains an XGBoost model with the corresponding parameters.
With the test set, the absolute error of the model is determined and returned.

All values for each task are gathered and ordered from low to high.
Depending on the kind of metric used, either the first or the last metric in the ordered list of values is taken.
For instance, if the mean error is used as the metric, we want to get the lowest value since this corresponds to the best performing model.
On the other hand, if accuracy is used as e metric, we want to get the highest value.

It is important to note that in this project the training of XGBoost models is not done in parallel.
The Spark operations are done on the list of different parameters instead.

### Training
When we have found the best metric and the corresponding parameter values, we train one final model.
This model is trained on the complete training dataset, which is also used for determining the best parameter values, and evaluated with the testing dataset.
When this point is reached, the model can start predicting values based on streaming data.

## Prediction
The model which was trained using the best parameter values is used to make predictions on 'new' data. The prediction model processes real-time incoming data. Kafka is used as a message broker for this real time communication. In this project, the real-time data is mimicked by an application that publishes data to Kafka. This is not different from a real-life application where people can send in message via a website or smartphone app.

The data that is used for the prediction in the current implementation originates from the database.
This is the part of the dataset which is not used for the parameter optimization and training process of the model.

### Kafka Set-up
All data for the prediction is transmitted via the topic, *actor_items*. The messages transmitted on this channel can be divided over multiple partitions and the partitions can be divided over multiple message brokers. This makes it possible to achieve maximum throughput. In order to optimize the message transmission even further, AVRO schemas are used. These AVRO schemas transform the messages to bits to minimize the message size. In the master, the messages are serialized into a format that can be used by the model.

The Kafka message broker consists of two applications: the producer and the consumer.
The producer retrieves the data from the database and processes the data into a correct data format for the model before pushing it to the message queue.
The processed data contains all features which the training data contained as well, and an extra column named *nconst*, which is the key that represents the actor id.

The consumer receives the data from the message queue and gives it to the model, which is trained with best parameter values, for prediction. Predictions made for each actor are saved in the database in a separate table.

## Future Work
Currently, processing the streaming data (making predictions) is done after the batch data is processed (parameter optimization and training the model).
Optimally, these two tasks were executed in parallel as well.
This would make the software more convenient to use, because there is no need to wait for the batch processing to be finished and would optimize the performance of the model.

With this implementation, there is one major problem: processing batch data has to be finished before streaming data can be processed.
Naturally, before predictions can be made, the model has to be trained.
A possible solution would be to keep track of different versions of the XGBoost model.

When streaming data is coming in, the complete dataset keeps on expanding.
Currently, this data is never used to improve the model.
If the model is trained in parallel to processing streaming data, the model would become better and better over time, since it will have more data to use for training.
When a new model is trained, with optimized parameters, the model used to make predictions on streaming data can be replaced with the new one.
A requirement for this is that the first time the software starts, a model is either provided or the user has to wait until the first model is trained on the batch data before it can start process streaming data.
