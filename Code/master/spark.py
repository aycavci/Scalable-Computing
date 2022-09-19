from pyspark import RDD
import pyspark.sql
from pyspark.sql import SparkSession

from services.regressor_service import RegressorService
from pyspark.sql.functions import col, expr

from pyspark.sql.types import StringType
from pyspark.sql.avro.functions import from_avro



class Spark:
    spark: SparkSession
    param_grid: RDD

    STREAMING_DATA_LIMIT = 10000

    def __init__(self):
        self.spark = SparkSession.builder \
            .appName('Master SQL') \
            .config('log4j.rootCategory', 'ERROR') \
            .config('spark.driver.bindAddress', '172.20.98.113') \
            .config('spark.jars.packages', 'org.apache.spark:spark-sql-kafka-0-10_2.12:3.1.1,org.apache.spark:spark-avro_2.12:3.1.1') \
            .config('spark.sql.streaming.forceDeleteTempCheckpointLocation', True) \
            .config('spark.sql.debug.maxToStringFields', 100) \
            .config('spark.ui.port', '4040') \
            .getOrCreate()

    def __exit__(self):
        self.spark.stop()

    def set_param_grid(self, param_grid):
        self.param_grid = self.spark.sparkContext.parallelize(param_grid)

    def train_model_k_fold(self, param_grid):
        self.set_param_grid(param_grid)

        regressor = RegressorService()
        # train all models
        results = regressor.execute(self.param_grid)

        # get results
        sums = results.aggregateByKey((0, 0), lambda a,b: (a[0] + b,    a[1] + 1), \
            lambda a,b: (a[0] + b[0], a[1] + b[1]))
        avgs = sums.mapValues(lambda v: v[0]/v[1])
        best_model = avgs.reduce(lambda a, b: a if a[1] < b[1] else b)

        print("best model:")
        print(best_model)

        params = best_model[0].split(":")
        self.best_eta = params[0]
        self.best_gamma = params[1]
        self.best_max_depth = params[2]

    def process_stream_data(self):
        regressor = RegressorService()
        regressor.train_streaming_model(self.best_eta, \
            self.best_gamma, \
            self.best_max_depth, \
            self.STREAMING_DATA_LIMIT)

        jsonFormatSchema = \
            open("../real-time-producer/schema/ActorItem.avro", "r").read()

        avroDF = self.spark \
            .readStream \
            .format("kafka") \
            .option("kafka.bootstrap.servers", "localhost:9092") \
            .option("mode", "PERMISSIVE") \
            .option("subscribe", "actor_items") \
            .load() \
            .withColumn('key', col("key").cast(StringType())) \
            .withColumn('fixedValue', expr("substring(value, 6, length(value)-5)")) \
            .select(from_avro("fixedValue", jsonFormatSchema).alias("actor")) \
            .select("actor.*") \
            .select("nconst", "birthyear", "title_count", "votes_avg", "is_adult", "release_year_min", "release_year_max", "release_year_avg", "title_type_movie", "title_type_short", "title_type_tv_episode", "title_type_tv_mini_series", "title_type_tv_movie", "title_type_tv_series", "title_type_tv_short", "title_type_tv_special", "title_type_video", "title_type_video_game", "role_category_actor", "role_category_archive_footage", "role_category_archive_sound", "role_category_cinematographer", "role_category_composer", "role_category_director", "role_category_editor", "role_category_producer", "role_category_production_designer", "role_category_self", "role_category_writer", "genre_action", "genre_adult", "genre_adventure", "genre_animation", "genre_biography", "genre_comedy", "genre_crime", "genre_documentary", "genre_drama", "genre_family", "genre_fantasy", "genre_film_noir", "genre_game_show", "genre_history", "genre_horror", "genre_music", "genre_musical", "genre_mystery", "genre_news", "genre_reality_tv", "genre_romance", "genre_sci_fi", "genre_short", "genre_sport", "genre_talk_show", "genre_thriller", "genre_war", "genre_western")


        stream = avroDF.writeStream.foreach(lambda row: regressor.predict(row)).start()

        stream.awaitTermination()
