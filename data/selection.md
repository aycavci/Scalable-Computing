# Data preparation
This file describes the data preparation steps used to obtain the used data set.

# forward postgres port
kubectl port-forward --address 0.0.0.0 service/test-db-postgresql-ha-pgpool 5432:5432

6iPPKqIl8p

## Modify original dataset
The original dataset contained the string "\N" in fields that were empty. These fields have been replace by an empty string using the command below. Also the header line of the original dataset was removed before applying the data selction.
# don't forget to remove the header(first line) from the modified files.
```
sed 's/\\\N//g' original.file.tsv > original.file.modified.tsv
```



## SQL data selection queries
The dataset has not been used completely. The queries in the following code block have been used to obtain the used collection.
```
#
# ratings per movie
#
CREATE TABLE titleRatingTEMP(tconst VARCHAR, averageRating VARCHAR, numVotes VARCHAR);

CREATE TABLE titleRating(tconst VARCHAR, averageRating FLOAT, numVotes INT);

\copy titleRatingTEMP from title.ratings.tsv/title.ratings.modified.tsv DELIMITER E'\t';

INSERT INTO titleRating(tconst, averageRating, numVotes)
SELECT 	titleRatingTEMP.tconst,
	NULLIF(titleRatingTEMP.averageRating, '')::float,
	NULLIF(titleRatingTEMP.numVotes, '')::int
FROM titleRatingTEMP ORDER BY numVotes DESC limit 150000;

DROP TABLE titleRatingTEMP;

#
# movie information
#
CREATE TABLE titleTEMP(tconst VARCHAR, titleType VARCHAR, primaryTitle VARCHAR, originalTitle VARCHAR, isAdult VARCHAR, startYear VARCHAR, endYear VARCHAR, runtimeMinutes VARCHAR, genres VARCHAR);

CREATE TABLE title(tconst VARCHAR, titleType VARCHAR, primaryTitle VARCHAR, originalTitle VARCHAR, isAdult INT, startYear INT, endYear INT, runtimeMinutes INT, genres VARCHAR);

\copy titleTEMP from title.basics.tsv/title.basics.modified.tsv DELIMITER E'\t';

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


#
# actors staringin movies
#
CREATE TABLE titlePrincipalTEMP(tconst VARCHAR, ordering VARCHAR, nconst VARCHAR, category VARCHAR, job VARCHAR, characters VARCHAR);

CREATE TABLE titlePrincipal(tconst VARCHAR, ordering INT, nconst VARCHAR, category VARCHAR, job VARCHAR, characters VARCHAR);

\copy titlePrincipalTEMP from title.principals.tsv/title.principals.modified.tsv DELIMITER E'\t';

INSERT INTO titlePrincipal(tconst, ordering, nconst, category, job, characters)
SELECT 	titlePrincipalTEMP.tconst,
	NULLIF(titlePrincipalTEMP.ordering, '')::int,
	titlePrincipalTEMP.nconst,
	titlePrincipalTEMP. category,
	titlePrincipalTEMP.job,
	titlePrincipalTEMP.characters
FROM titlePrincipalTEMP INNER JOIN titleRating ON titlePrincipalTEMP.tconst = titleRating.tconst;

DROP TABLE titlePrincipalTEMP;


#
# actors
#
CREATE TABLE actorTEMP(nconst VARCHAR, primaryName VARCHAR, birthYear VARCHAR, deathYear VARCHAR, primaryProfession VARCHAR, knownForTitles VARCHAR);

CREATE TABLE actor(nconst VARCHAR, name VARCHAR, birthYear INT, deathYear INT, primaryProfession VARCHAR, knownForTitles VARCHAR);

\copy actorTEMP from name.basics.tsv/name.basics.modified.tsv DELIMITER E'\t';

INSERT INTO actor(nconst, name, birthYear, deathYear, primaryProfession, knownForTitles)
SELECT 	actorTEMP.nconst,
	actorTEMP.primaryName,
	NULLIF(actorTEMP.birthYear, '')::int,
	NULLIF(actorTEMP.deathYear, '')::int,
	actorTEMP.primaryProfession,
	actorTEMP.knownForTitles
FROM actorTEMP INNER JOIN titlePrincipal ON actorTEMP.nconst = titlePrincipal.nconst;

DROP TABLE actorTEMP;
```





## Export data
The following commands were executed in psql to obtain new tsv data files for the data selection.
```
\copy (SELECT * FROM actor) TO actor.tsv WITH DELIMITER E'\t';
\copy (SELECT * FROM title) TO title.tsv WITH DELIMITER E'\t';
\copy (SELECT * FROM titlePrincipal) TO title.principal.tsv WITH DELIMITER E'\t';
\copy (SELECT * FROM titleRating) TO title.rating.tsv WITH DELIMITER E'\t';
```

## queries for creating final selction
```
\c imdb


--
-- table 1
--
CREATE TABLE actorCount(
  nconst VARCHAR,
  title_count INT,
  movie_title_avg FLOAT,
  short_title_avg FLOAT,
  tv_episode_title_avg FLOAT,
  tv_mini_series_title_avg FLOAT,
  tv_movie_title_avg FLOAT,
  tv_series_title_avg FLOAT,
  tv_short_title_avg FLOAT,
  tv_special_title_avg FLOAT,
  video_title_avg FLOAT,
  video_game_title_avg FLOAT,
  actor_role_avg FLOAT,
  archive_footage_role_avg FLOAT,
  archive_sound_role_avg FLOAT,
  cinematographer_role_avg FLOAT,
  composer_role_avg FLOAT,
  director_role_avg FLOAT,
  editor_role_avg FLOAT,
  producer_role_avg FLOAT,
  production_designer_role_avg FLOAT,
  self_role_avg FLOAT,
  writer_role_avg FLOAT
);

INSERT INTO actorCount(
  nconst,
  title_count,
  movie_title_avg,
  short_title_avg,
  tv_episode_title_avg,
  tv_mini_series_title_avg,
  tv_movie_title_avg,
  tv_series_title_avg,
  tv_short_title_avg,
  tv_special_title_avg,
  video_title_avg,
  video_game_title_avg,
  actor_role_avg,
  archive_footage_role_avg,
  archive_sound_role_avg,
  cinematographer_role_avg,
  composer_role_avg,
  director_role_avg,
  editor_role_avg,
  producer_role_avg,
  production_designer_role_avg,
  self_role_avg,
  writer_role_avg
)
select
  titlePrincipal.nconst,
  count(titlePrincipal.tconst) as title_count,
  avg(case title.titletype when 'movie' then 1 else 0 end) as movie_title_count,
  avg(case title.titletype when 'short' then 1 else 0 end) as short_title_count,
  avg(case title.titletype when 'tvEpisode' then 1 else 0 end) as tv_episode_title_count,
  avg(case title.titletype when 'tvMiniSeries' then 1 else 0 end) as tv_mini_series_title_count,
  avg(case title.titletype when 'tvMovie' then 1 else 0 end) as tv_movie_title_count,
  avg(case title.titletype when 'tvSeries' then 1 else 0 end) as tv_series_title_count,
  avg(case title.titletype when 'tvShort' then 1 else 0 end) as tv_short_title_count,
  avg(case title.titletype when 'tvSpecial' then 1 else 0 end) as tv_special_title_count,
  avg(case title.titletype when 'video' then 1 else 0 end) as video_title_count,
  avg(case title.titletype when 'videoGame' then 1 else 0 end) as video_game_title_count,
  avg(case titlePrincipal.category when 'actor' then 1 when 'actress'then 1 else 0 end) as actor_count,
  avg(case titlePrincipal.category when 'archive_footage' then 1 else 0 end) as archive_footage_count,
  avg(case titlePrincipal.category when 'archive_sound' then 1 else 0 end) as archive_sound_count,
  avg(case titlePrincipal.category when 'cinematographer' then 1 else 0 end) as cinematographer_count,
  avg(case titlePrincipal.category when 'composer' then 1 else 0 end) as composer_count,
  avg(case titlePrincipal.category when 'director' then 1 else 0 end) as director_count,
  avg(case titlePrincipal.category when 'editor' then 1 else 0 end) as editor_count,
  avg(case titlePrincipal.category when 'producer' then 1 else 0 end) as producer_count,
  avg(case titlePrincipal.category when 'production_designer' then 1 else 0 end) as production_designer_count,
  avg(case titlePrincipal.category when 'self' then 1 else 0 end) as self_count,
  avg(case titlePrincipal.category when 'writer' then 1 else 0 end) as writer_count
from titlePrincipal
join title on titlePrincipal.tconst = title.tconst
group by titlePrincipal.nconst;


--
-- Table 2
--create table actorVotes(
  nconst varchar,
  avg_num_votes float,
  avg_rating float
);

insert into actorVotes(
  nconst,
  avg_num_votes,
  avg_rating
)
select
  titlePrincipal.nconst,
  avg(titlerating.numvotes),
  avg(titlerating.averagerating)
from
  titlePrincipal
join
  titlerating on titlerating.tconst = titlePrincipal.tconst
group by
  titlePrincipal.nconst;

--
-- table 3
--
create table actorAdult(
  nconst varchar,
  avg_is_adult float,
  min_relaese_year int,
  max_relaese_year int,
  avg_release_year float
);

insert into actorAdult(nconst, avg_is_adult, min_relaese_year, max_relaese_year, avg_release_year)
select
  distinct on (actor.nconst)
  actor.nconst,
  avg(title.isadult),
  min(title.startyear),
  max(title.startyear),
  avg(title.startyear)
from
  actor
join
  titlePrincipal on titlePrincipal.nconst = actor.nconst
join
  title on title.tconst = titlePrincipal.tconst
group by
  actor.nconst;


--
-- support table
--
create table titleGenre
  (
    id serial,
    tconst varchar,
    genre varchar
  );

insert into  titleGenre(tconst, genre)
select
  title.tconst,
  split_part(title.genres, ',', 1) as genre
from
  title
union
select
  title.tconst,
  split_part(title.genres, ',', 2) as genre
from
  title
union
select
  title.tconst,
  split_part(title.genres, ',', 3) as genre
from
  title;

delete from titleGenre where genre = '';


--
-- table 4
--
create table actorGenre(
  nconst varchar,
  action_genre_avg float,
  adult_genre_avg float,
  adventure_genre_avg float,
  animation_genre_avg float,
  biography_genre_avg float,
  comedy_genre_avg float,
  crime_genre_avg float,
  documentary_genre_avg float,
  drama_genre_avg float,
  family_genre_avg float,
  fantasy_genre_avg float,
  film_noir_genre_avg float,
  game_show_genre_avg float,
  history_genre_avg float,
  horror_genre_avg float,
  music_genre_avg float,
  musical_genre_avg float,
  mystery_genre_avg float,
  news_genre_avg float,
  reality_tv_genre_avg float,
  romance_genre_avg float,
  sci_fi_genre_avg float,
  short_genre_avg float,
  sport_genre_avg float,
  talk_show_genre_avg float,
  thriller_genre_avg float,
  war_genre_avg float,
  western_genre_avg float
);


INSERT INTO actorGenre(
    nconst,
    action_genre_avg,
    adult_genre_avg,
    adventure_genre_avg,
    animation_genre_avg,
    biography_genre_avg,
    comedy_genre_avg,
    crime_genre_avg,
    documentary_genre_avg,
    drama_genre_avg,
    family_genre_avg,
    fantasy_genre_avg,
    film_noir_genre_avg,
    game_show_genre_avg,
    history_genre_avg,
    horror_genre_avg,
    music_genre_avg,
    musical_genre_avg,
    mystery_genre_avg,
    news_genre_avg,
    reality_TV_genre_avg,
    romance_genre_avg,
    sci_fi_genre_avg,
    short_genre_avg,
    sport_genre_avg,
    talk_show_genre_avg,
    thriller_genre_avg,
    war_genre_avg,
    western_genre_avg
  )
select
  actor.nconst,
  avg(case titleGenre.genre when 'Action' then 1 else 0 end) as action_avg,
  avg(case titleGenre.genre when 'Adult' then 1 else 0 end) as adult_avg,
  avg(case titleGenre.genre when 'Adventure' then 1 else 0 end) as adventure_avg,
  avg(case titleGenre.genre when 'Animation' then 1 else 0 end) as animation_avg,
  avg(case titleGenre.genre when 'Biograhpy' then 1 else 0 end) as biograhpy_avg,
  avg(case titleGenre.genre when 'Comedy' then 1 else 0 end) as comedy_avg,
  avg(case titleGenre.genre when 'Crime' then 1 else 0 end) as crime_avg,
  avg(case titleGenre.genre when 'Documentary' then 1 else 0 end) as documentary_avg,
  avg(case titleGenre.genre when 'Drama' then 1 else 0 end) as drama_avg,
  avg(case titleGenre.genre when 'Family' then 1 else 0 end) as family_avg,
  avg(case titleGenre.genre when 'Fantasy' then 1 else 0 end) as fantasy_avg,
  avg(case titleGenre.genre when 'Film-Noir' then 1 else 0 end) as film_noir_avg,
  avg(case titleGenre.genre when 'Game-Show' then 1 else 0 end) as game_show_avg,
  avg(case titleGenre.genre when 'History' then 1 else 0 end) as history_avg,
  avg(case titleGenre.genre when 'Horror' then 1 else 0 end) as horror_avg,
  avg(case titleGenre.genre when 'Music' then 1 else 0 end) as music_avg,
  avg(case titleGenre.genre when 'Musical' then 1 else 0 end) as musical_avg,
  avg(case titleGenre.genre when 'Mystery' then 1 else 0 end) as mystery_avg,
  avg(case titleGenre.genre when 'News' then 1 else 0 end) as news_avg,
  avg(case titleGenre.genre when 'Reality-TV' then 1 else 0 end) as reality_tv_avg,
  avg(case titleGenre.genre when 'Romance' then 1 else 0 end) as romance_avg,
  avg(case titleGenre.genre when 'Sci-Fi' then 1 else 0 end) as sci_fi_avg,
  avg(case titleGenre.genre when 'Short' then 1 else 0 end) as short_avg,
  avg(case titleGenre.genre when 'Sport' then 1 else 0 end) as sport_avg,
  avg(case titleGenre.genre when 'Talk-Show' then 1 else 0 end) as talk_show_avg,
  avg(case titleGenre.genre when 'Thriller' then 1 else 0 end) as thriller_avg,
  avg(case titleGenre.genre when 'War' then 1 else 0 end) as war_avg,
  avg(case titleGenre.genre when 'Western' then 1 else 0 end) as western_avg
from
  actor
join
  titlePrincipal on titlePrincipal.nconst = actor.nconst
join
  titleGenre on titleGenre.tconst = titlePrincipal.tconst
group by
  actor.nconst;
```

## combining all the tables into one table
```
\c imdb

create table final_result(
  nconst varchar,
  birthyear int,
  deathyear int,
  title_count int,
  votes_avg float,
  is_adult float,
  release_year_min int,
  release_year_max int,
  release_year_avg float,
  title_type_movie float,
  title_type_short float,
  title_type_tv_episode float,
  title_type_tv_mini_series float,
  title_type_tv_movie float,
  title_type_tv_series float,
  title_type_tv_short float,
  title_type_tv_special float,
  title_type_video float,
  title_type_video_game float,
  role_category_actor float,
  role_category_archive_footage float,
  role_category_archive_sound float,
  role_category_cinematographer float,
  role_category_composer float,
  role_category_director float,
  role_category_editor float,
  role_category_producer float,
  role_category_production_designer float,
  role_category_self float,
  role_category_writer float,
  genre_action float,
  genre_adult float,
  genre_adventure float,
  genre_animation float,
  genre_biography float,
  genre_comedy float,
  genre_crime float,
  genre_documentary float,
  genre_drama float,
  genre_family float,
  genre_fantasy float,
  genre_film_noir float,
  genre_game_show float,
  genre_history float,
  genre_horror float,
  genre_music float,
  genre_musical float,
  genre_mystery float,
  genre_news float,
  genre_reality_tv float,
  genre_romance float,
  genre_scif_fi float,
  genre_short float,
  genre_sport float,
  genre_talk_show float,
  genre_thriller float,
  genre_war float,
  genre_western float
);

insert into final_result(
  nconst,
  birthyear,
  title_count,
  votes_avg,
  is_adult,
  release_year_min,
  release_year_max,
  release_year_avg,
  title_type_movie,
  title_type_short,
  title_type_tv_episode,
  title_type_tv_mini_series,
  title_type_tv_movie,
  title_type_tv_series,
  title_type_tv_short,
  title_type_tv_special,
  title_type_video,
  title_type_video_game,
  role_category_actor,
  role_category_archive_footage,
  role_category_archive_sound,
  role_category_cinematographer,
  role_category_composer,
  role_category_director,
  role_category_editor,
  role_category_producer,
  role_category_production_designer,
  role_category_self,
  role_category_writer,
  genre_action,
  genre_adult,
  genre_adventure,
  genre_animation,
  genre_biography,
  genre_comedy,
  genre_crime,
  genre_documentary,
  genre_drama,
  genre_family,
  genre_fantasy,
  genre_film_noir,
  genre_game_show,
  genre_history,
  genre_horror,
  genre_music,
  genre_musical,
  genre_mystery,
  genre_news,
  genre_reality_tv,
  genre_romance,
  genre_scif_fi,
  genre_short,
  genre_sport,
  genre_talk_show,
  genre_thriller,
  genre_war,
  genre_western
)
select
  actorCount.nconst as nconst,
  actor.birthyear as birthyear,
  actorCount.title_count as title_count,
  actorVotes.avg_num_votes as votes_avg,
  actorAdult.avg_is_adult as is_adult,
  actorAdult.min_relaese_year as release_year_min,
  actorAdult.max_relaese_year as release_year_max,
  actorAdult.avg_release_year as release_year_avg,
  actorCount.movie_title_avg as title_type_movie,
  actorCount.short_title_avg as title_type_short,
  actorCount.tv_episode_title_avg as title_type_tv_episode,
  actorCount.tv_mini_series_title_avg as title_type_tv_mini_series,
  actorCount.tv_movie_title_avg as title_type_tv_movie,
  actorCount.tv_series_title_avg as title_type_tv_series,
  actorCount.tv_short_title_avg as title_type_tv_short,
  actorCount.tv_special_title_avg as title_type_tv_special,
  actorCount.video_title_avg as title_type_video,
  actorCount.video_game_title_avg as title_type_video_game,
  actorCount.actor_role_avg as role_category_actor,
  actorCount.archive_footage_role_avg as role_category_archive_footage,
  actorCount.archive_sound_role_avg as role_category_archive_sound,
  actorCount.cinematographer_role_avg as role_category_cinematographer,
  actorCount.composer_role_avg as role_category_composer,
  actorCount.director_role_avg as role_category_director,
  actorCount.editor_role_avg as role_category_editor,
  actorCount.producer_role_avg as role_category_producer,
  actorCount.production_designer_role_avg as role_category_production_designer,
  actorCount.self_role_avg as role_category_self,
  actorCount.writer_role_avg as role_category_writer,
  actorGenre.action_genre_avg as genre_action,
  actorGenre.adult_genre_avg as genre_adult,
  actorGenre.adventure_genre_avg as genre_adventure,
  actorGenre.animation_genre_avg as genre_animation,
  actorGenre.biography_genre_avg as genre_biography,
  actorGenre.comedy_genre_avg as genre_comedy,
  actorGenre.crime_genre_avg as genre_crime,
  actorGenre.documentary_genre_avg as genre_documentary,
  actorGenre.drama_genre_avg as genre_drama,
  actorGenre.family_genre_avg as genre_family,
  actorGenre.fantasy_genre_avg as genre_fantasy,
  actorGenre.film_noir_genre_avg as genre_film_noir,
  actorGenre.game_show_genre_avg as genre_game_show,
  actorGenre.history_genre_avg as genre_history,
  actorGenre.horror_genre_avg as genre_horror,
  actorGenre.music_genre_avg as genre_music,
  actorGenre.musical_genre_avg as genre_musical,
  actorGenre.mystery_genre_avg as genre_mystery,
  actorGenre.news_genre_avg as genre_news,
  actorGenre.reality_TV_genre_avg as genre_reality_tv,
  actorGenre.romance_genre_avg as genre_romance,
  actorGenre.sci_fi_genre_avg as genre_scif_fi,
  actorGenre.short_genre_avg as genre_short,
  actorGenre.sport_genre_avg as genre_sport,
  actorGenre.talk_show_genre_avg as genre_talk_show,
  actorGenre.thriller_genre_avg as genre_thriller,
  actorGenre.war_genre_avg as genre_war,
  actorGenre.western_genre_avg as genre_western
from
  actorCount
join
  actor on actor.nconst = actorCount.nconst
join
  actorVotes on actorCount.nconst = actorVotes.nconst
join
  actorAdult on actorAdult.nconst = actorVotes.nconst
join
  actorGenre on actorGenre.nconst = actorCount.nconst;
```

## export data
```
\copy (SELECT * FROM final_result) TO out.tsv WITH DELIMITER E'\t';
```
