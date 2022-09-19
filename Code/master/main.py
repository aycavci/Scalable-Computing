from services import parameter_service
import spark


# Create K-fold param-grid
param_grid = parameter_service.generate_k_fold_search_space(3, 10000);

for grid in param_grid:
    print(grid)

spark = spark.Spark()

spark.train_model_k_fold(param_grid)
spark.process_stream_data()
