CREATE DATABASE imdb;

\c imdb;

--
-- ratings per movie
--
CREATE TABLE titleRatingTEMP(tconst VARCHAR, averageRating VARCHAR, numVotes VARCHAR);

CREATE TABLE titleRating(tconst VARCHAR, averageRating FLOAT, numVotes INT);

\copy titleRatingTEMP from dataset/title.rating.tsv DELIMITER E'\t';

INSERT INTO titleRating(tconst, averageRating, numVotes)
SELECT 	titleRatingTEMP.tconst,
	NULLIF(titleRatingTEMP.averageRating, '')::float,
	NULLIF(titleRatingTEMP.numVotes, '')::int
FROM titleRatingTEMP ORDER BY numVotes DESC limit 150000;

DROP TABLE titleRatingTEMP;

--
-- movie information
--
CREATE TABLE titleTEMP(tconst VARCHAR, titleType VARCHAR, primaryTitle VARCHAR, originalTitle VARCHAR, isAdult VARCHAR, startYear VARCHAR, endYear VARCHAR, runtimeMinutes VARCHAR, genres VARCHAR);

CREATE TABLE title(tconst VARCHAR, titleType VARCHAR, primaryTitle VARCHAR, originalTitle VARCHAR, isAdult INT, startYear INT, endYear INT, runtimeMinutes INT, genres VARCHAR);

\copy titleTEMP from dataset/title.tsv DELIMITER E'\t';

INSERT INTO title(tconst, titleType, primaryTitle, originalTitle, isAdult, startYear, endYear, runtimeMinutes, genres)
SELECT 	titleTEMP.tconst,
	titleTEMP.titleType,
	titleTEMP.primaryTitle,
	titleTEMP.originalTitle,
	NULLIF(titleTEMP.isAdult, '')::int,
	NULLIF(titleTEMP.startYear, '')::int,
	NULLIF(titleTEMP.endYear, '')::int,
	NULLIF(titleTEMP.runtimeMinutes, '')::int,
	titleTEMP.genres
FROM titleTEMP INNER JOIN titleRating ON titleTEMP.tconst = titleRating.tconst;

DROP TABLE titleTEMP;


--
-- actors staringin movies
--
CREATE TABLE titlePrincipalTEMP(tconst VARCHAR, ordering VARCHAR, nconst VARCHAR, category VARCHAR, job VARCHAR, characters VARCHAR);

CREATE TABLE titlePrincipal(tconst VARCHAR, ordering INT, nconst VARCHAR, category VARCHAR, job VARCHAR, characters VARCHAR);

\copy titlePrincipalTEMP from dataset/title.principal.tsv DELIMITER E'\t';

INSERT INTO titlePrincipal(tconst, ordering, nconst, category, job, characters)
SELECT 	titlePrincipalTEMP.tconst,
	NULLIF(titlePrincipalTEMP.ordering, '')::int,
	titlePrincipalTEMP.nconst,
	titlePrincipalTEMP. category,
	titlePrincipalTEMP.job,
	titlePrincipalTEMP.characters
FROM titlePrincipalTEMP INNER JOIN titleRating ON titlePrincipalTEMP.tconst = titleRating.tconst;

DROP TABLE titlePrincipalTEMP;


--
-- actors
--
CREATE TABLE actorTEMP(nconst VARCHAR, primaryName VARCHAR, birthYear VARCHAR, deathYear VARCHAR, primaryProfession VARCHAR, knownForTitles VARCHAR);

CREATE TABLE actor(nconst VARCHAR, name VARCHAR, birthYear INT, deathYear INT, primaryProfession VARCHAR, knownForTitles VARCHAR);

\copy actorTEMP from dataset/actor.tsv DELIMITER E'\t';

INSERT INTO actor(nconst, name, birthYear, deathYear, primaryProfession, knownForTitles)
SELECT 	actorTEMP.nconst,
	actorTEMP.primaryName,
	NULLIF(actorTEMP.birthYear, '')::int,
	NULLIF(actorTEMP.deathYear, '')::int,
	actorTEMP.primaryProfession,
	actorTEMP.knownForTitles
FROM actorTEMP INNER JOIN titlePrincipal ON actorTEMP.nconst = titlePrincipal.nconst;

DROP TABLE actorTEMP;
