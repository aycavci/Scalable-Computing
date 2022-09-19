from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker

from pyspark import RDD

import pandas as pd
import xgboost as xgb
import random
from statistics import mean

class RegressorService:
    LABEL_COLUMN_NAME = 'rating_avg'
    FEATURES_COLUMN_NAME = 'features'
    PREDICTION_COLUMN_NAME = 'prediction'
    PARAMETERS_COLUMN_NAME = 'parameter_values'
    METRIC_COLUMN_NAME = 'performance'

    engine = create_engine("postgresql://postgres:postgres@127.0.0.1:5432/imdb")
    session = sessionmaker(bind=engine)

    def execute(self, param_settings):
        return param_settings.map(lambda row: self.evaluate_model(row))

    def get_data(self, limit, offset):
        query = "SELECT DISTINCT * FROM dataset WHERE dataset IS NOT NULL LIMIT " \
            + str(limit) + " offset " + str(offset) + ";"

        return pd.read_sql(query, self.session().bind)

    def extract(self, dataset):
        # Remove the columns with type 'object' from the dataframe,
        # i.e., remove the row IDs.
        dataset = dataset.select_dtypes(exclude=['object'])

        # Get the feature names by extracting the column names from the
        # dataframe and excluding the label column name.
        feature_names = dataset.columns.values.tolist()
        feature_names.remove(self.LABEL_COLUMN_NAME)

        return dataset[feature_names], list(dataset[self.LABEL_COLUMN_NAME])

    def load_data(self, limit, offset):
        test_set = self.get_data(limit, 0)
        train_set = self.get_data(limit, offset)
        self.train_features, self.train_labels = self.extract(train_set)
        self.test_features, self.test_labels = self.extract(test_set)

    def train_model(self, eta, gamma, max_depth, features, labels):
        model = xgb.XGBRegressor(eta = eta, \
            gamma = gamma, \
            max_depth = max_depth)

        model.fit(features, labels)
        return model

    def test_model(self, model, features, labels):
        predictions = model.predict(features)
        errors = abs(predictions - labels)
        return float(mean(errors))

    def evaluate_model(self, param_settings):
        self.load_data(param_settings[3], param_settings[4])

        model = self.train_model( \
            param_settings[0], \
            param_settings[1], \
            param_settings[2], \
            self.train_features, \
            self.train_labels)

        result = self.test_model(model, \
            self.test_features, \
            self.test_labels)

        return (str(param_settings[0]) + ":" \
            + str(param_settings[1]) + ":" \
            + str(param_settings[2]), result)

    def train_streaming_model(self, eta, gamma, max_depth, limit = 10000):
        train_data = self.get_data(limit, 0)
        train_features, train_labels = self.extract(train_data)
        self.streaming_model = self.train_model(eta, \
            gamma, \
            max_depth, \
            train_features, \
            train_labels)

    def predict(self, features):
        featuresPandas = pd.DataFrame(features.asDict(), index=[0])
        featuresPandas.drop('nconst', axis='columns', inplace=True)

        prediction = self.streaming_model.predict(featuresPandas)

        print(str(features.__getattr__("nconst")) + " average rating: " + str(prediction))
