from itertools import product

import numpy as np

# Specify all values for the parameters we want to test.
parameters = [
    list(np.arange(0, 1.1, 0.5)),  # eta
    list(np.arange(0, 11, 5)),  # gamma
    list(np.arange(1, 12, 5))  # max_depth
]

#
# Creates a search space for the xgboost algorithm with options for the
# parameters eta, gamma and max_depth. On top of that it also creates blocks
# of data to be used for the training purpose. The block of totalRecords is
# divided into k + 1 blocks of data. The first block is always used as the
# test set and the other blocks are used to train the same model k times.
#
def generate_k_fold_search_space(k = 3, totalRecords = 10000):
    partitionNum = totalRecords / (k + 1);
    parameter_model_list = []

    parameters.append(list(np.array([partitionNum])))
    parameters.append(list(np.arange(partitionNum, partitionNum + k * partitionNum, partitionNum)))

    # create list of param tuples for each combination
    # tupple: (numTrees, maxDepth, subSamplingRate, limit, offset)
    for values in product(*parameters):
        parameter_model_list.append(
            (
                round(values[0], 1).item(),
                int(values[1]),
                int(values[2]),
                int(values[3]),
                int(values[4])
            )
        )

    return [ \
        (0.0, 10, 11, 2500, 2500), \
        (0.6, 10, 11, 2500, 5000), \
        (0.0, 10, 11, 2500, 7500)
        ]
    return parameter_model_list
