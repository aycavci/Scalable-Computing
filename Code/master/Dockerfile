FROM godatadriven/pyspark

WORKDIR /jobs

COPY requirements.txt ./

RUN apt-get update && apt-get install -y build-essential libpq-dev
RUN pip install --no-cache-dir -r requirements.txt

COPY . .
