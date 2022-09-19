using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace real_time_producer
{
    public class ActorResultWorker : BackgroundService
    {
        private readonly ILogger<ActorResultWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaClient _kafkaClient;

        public ActorResultWorker(ILogger<ActorResultWorker> logger,
            IServiceScopeFactory scopeFactory,
            KafkaClient kafkaClient)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _kafkaClient = kafkaClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // initialize with a because it is always smaller than "n*".
            string prevNconst = "a";
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}",
                    DateTimeOffset.Now);

                // now use the db context to get a single instance of the actor data.
                using(var scope = _scopeFactory.CreateScope())
                {
                    var imdbDb = scope.ServiceProvider
                        .GetRequiredService<ActorDbContext>();

                    var actorData = imdbDb.ActorDataset
                        .Where(a => String.Compare(a.Nconst, prevNconst) > 0)
                        .OrderBy(a => a.Nconst)
                        .Take(10);

                    foreach(ActorData actor in actorData)
                    {
                        _logger.LogInformation(actor.Nconst);
                    }

                    await _kafkaClient.Produce(
                        actorTypeTransform(actorData),
                        stoppingToken);

                    prevNconst = actorData.Last().Nconst;
                }

                await Task.Delay(10000, stoppingToken);
            }
        }

        private List<ActorItem> actorTypeTransform(
            IEnumerable<ActorData> records)
        {
            List<ActorItem> actors = new List<ActorItem>();

            foreach(ActorData record in records)
            {
                ActorItem actor = new ActorItem();

                actor.nconst = record.Nconst ?? default(string);
                actor.birthyear = record.Birthyear ?? default(int);
                actor.title_count = record.TitleCount ?? default(int);
                actor.votes_avg = record.VotesAvg ?? default(float);
                actor.rating_avg = record.RatingAvg ?? default(float);
                actor.is_adult = record.IsAdult ?? default(float);
                actor.release_year_min = record.ReleaseYearMin ?? default(int);
                actor.release_year_max = record.ReleaseYearMax ?? default(int);
                actor.release_year_avg = record.ReleaseYearAvg ?? default(float);
                actor.title_type_movie = record.TitleTypeMovie ?? default(float);
                actor.title_type_short = record.TitleTypeShort ?? default(float);
                actor.title_type_tv_episode = record.TitleTypeTvEpisode ?? default(float);
                actor.title_type_tv_mini_series = record.TitleTypeTvMiniSeries ?? default(float);
                actor.title_type_tv_movie = record.TitleTypeTvMovie ?? default(float);
                actor.title_type_tv_series = record.TitleTypeTvSeries ?? default(float);
                actor.title_type_tv_short = record.TitleTypeTvShort ?? default(float);
                actor.title_type_tv_special = record.TitleTypeTvSpecial ?? default(float);
                actor.title_type_video = record.TitleTypeVideo ?? default(float);
                actor.title_type_video_game = record.TitleTypeVideoGame ?? default(float);
                actor.role_category_actor = record.RoleCategoryActor ?? default(float);
                actor.role_category_archive_footage = record.RoleCategoryArchiveFootage ?? default(float);
                actor.role_category_archive_sound = record.RoleCategoryArchiveSound ?? default(float);
                actor.role_category_cinematographer = record.RoleCategoryCinematographer ?? default(float);
                actor.role_category_composer = record.RoleCategoryComposer ?? default(float);
                actor.role_category_director = record.RoleCategoryDirector ?? default(float);
                actor.role_category_editor = record.RoleCategoryEditor ?? default(float);
                actor.role_category_producer = record.RoleCategoryProducer ?? default(float);
                actor.role_category_production_designer = record.RoleCategoryProductionDesigner ?? default(float);
                actor.role_category_self = record.RoleCategorySelf ?? default(float);
                actor.role_category_writer = record.RoleCategoryWriter ?? default(float);
                actor.genre_action = record.GenreAction ?? default(float);
                actor.genre_adult = record.GenreAdult ?? default(float);
                actor.genre_adventure = record.GenreAdventure ?? default(float);
                actor.genre_animation = record.GenreAnimation ?? default(float);
                actor.genre_biography = record.GenreBiography ?? default(float);
                actor.genre_comedy = record.GenreComedy ?? default(float);
                actor.genre_crime = record.GenreCrime ?? default(float);
                actor.genre_documentary = record.GenreDocumentary ?? default(float);
                actor.genre_drama = record.GenreDrama ?? default(float);
                actor.genre_family = record.GenreFamily ?? default(float);
                actor.genre_fantasy = record.GenreFantasy ?? default(float);
                actor.genre_film_noir = record.GenreFilmNoir ?? default(float);
                actor.genre_game_show = record.GenreGameShow ?? default(float);
                actor.genre_history = record.GenreHistory ?? default(float);
                actor.genre_horror = record.GenreHorror ?? default(float);
                actor.genre_music = record.GenreMusic ?? default(float);
                actor.genre_musical = record.GenreMusical ?? default(float);
                actor.genre_mystery = record.GenreMystery ?? default(float);
                actor.genre_news = record.GenreNews ?? default(float);
                actor.genre_reality_tv = record.GenreRealityTv ?? default(float);
                actor.genre_romance = record.GenreRomance ?? default(float);
                actor.genre_sci_fi = record.GenreSciFi ?? default(float);
                actor.genre_short = record.GenreShort ?? default(float);
                actor.genre_sport = record.GenreSport ?? default(float);
                actor.genre_talk_show = record.GenreTalkShow ?? default(float);
                actor.genre_thriller = record.GenreThriller ?? default(float);
                actor.genre_war = record.GenreWar ?? default(float);
                actor.genre_western = record.GenreWestern ?? default(float);

                actors.Add(actor);
            }
            return actors;
        }

    }
}
