#!/bin/bash
#
# Color parameters
#
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

#
# Check kubernetes cluster is running
#
read -p "Is your kubernetes cluster running(yes/no)?" kuberCluster

if [ "$kuberCluster" != "yes" -a "$kuberCluster" != "y" ]
then
  echo -e "${RED}Make sure that you kubernetes cluster is ready!${NC}"
  exit
fi

#
# Check the Bitnami helm repo has been added
#
read -p "Did you install the Bitnami helm repo(yes/no)?" bitnami

if [ "$bitnami" != "yes" -a "$bitnami" != "y" ]
then
  echo -e "${BLUE}Installing helm repo"
  helm repo add bitnami https://charts.bitnami.com/bitnami
fi



#
# Setup postgres cluster
#
echo -e "${BLUE}Setting up the database cluster on kubernetes${NC}"
helm install imdb-db -f kubernetes/postgresValues.yaml bitnami/postgresql-ha

#
# Sleep 60 seconds to ensure kubernetes is setup.
#
echo -e "${BLUE}The setup will now sleep for 60 seconds to ensure kubernetes is setup.\nThereafter the database will be initialized.${NC}"
sleep 60


#
# seting env parameters for psql
#
export NODE_IP=$(kubectl get nodes --namespace default -o jsonpath="{.items[0].status.addresses[0].address}")
export NODE_PORT=$(kubectl get --namespace default -o jsonpath="{.spec.ports[0].nodePort}" services imdb-db-pgpool)

#
# Execute queries to load the data.
#

echo -e "${BLUE}Running queries to insert data into database.${NC}"

PGPASSWORD=postgres psql -h ${NODE_IP} -p ${NODE_PORT} -U postgres -d postgres -f data/initDB2.sql

echo -e "${BLUE}The imdb database is completely setup!${NC}"
echo "Connect to postgres on adress: ${NODE_IP} and via port: ${NODE_PORT}"

#
# Creating kafka
#

echo -e "${BLUE}Setting up Kafka${NC}"

helm repo add confluentinc https://confluentinc.github.io/cp-helm-charts/
helm repo update
helm install my-kafka confluentinc/cp-helm-charts -f kubernetes/kafkaValues.yaml

#
# create and run real-time-producer
#
echo -e "${BLUE}Creating real-time-producer${NC}"

kubectl run real-time-producer --image=sytseoegema/rug-sc
