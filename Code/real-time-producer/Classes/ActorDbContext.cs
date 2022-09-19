using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace real_time_producer
{
    public class ActorDbContext : DbContext
    {
        public ActorDbContext(DbContextOptions<ActorDbContext> options)
            : base(options)
        {
        }

        public DbSet<ActorData> ActorDataset { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "C.UTF-8");

            modelBuilder.Entity<ActorData>(entity =>
            {
              entity.HasNoKey();
              entity.ToTable("dataset");

              entity.Property(e => e.Nconst)
                  .HasColumnType("character varying")
                  .HasColumnName("nconst");

              entity.Property(e => e.Birthyear).HasColumnName("birthyear");

              entity.Property(e => e.TitleCount).HasColumnName("title_count");
              entity.Property(e => e.VotesAvg).HasColumnName("votes_avg");
              entity.Property(e => e.RatingAvg).HasColumnName("rating_avg");
              entity.Property(e => e.IsAdult).HasColumnName("is_adult");

              entity.Property(e => e.ReleaseYearAvg).HasColumnName("release_year_avg");
              entity.Property(e => e.ReleaseYearMax).HasColumnName("release_year_max");
              entity.Property(e => e.ReleaseYearMin).HasColumnName("release_year_min");

              entity.Property(e => e.TitleTypeMovie).HasColumnName("title_type_movie");
              entity.Property(e => e.TitleTypeShort).HasColumnName("title_type_short");
              entity.Property(e => e.TitleTypeTvEpisode).HasColumnName("title_type_tv_episode");
              entity.Property(e => e.TitleTypeTvMiniSeries).HasColumnName("title_type_tv_mini_series");
              entity.Property(e => e.TitleTypeTvMovie).HasColumnName("title_type_tv_movie");
              entity.Property(e => e.TitleTypeTvSeries).HasColumnName("title_type_tv_series");
              entity.Property(e => e.TitleTypeTvShort).HasColumnName("title_type_tv_short");
              entity.Property(e => e.TitleTypeTvSpecial).HasColumnName("title_type_tv_special");
              entity.Property(e => e.TitleTypeVideo).HasColumnName("title_type_video");
              entity.Property(e => e.TitleTypeVideoGame).HasColumnName("title_type_video_game");

              entity.Property(e => e.RoleCategoryActor).HasColumnName("role_category_actor");
              entity.Property(e => e.RoleCategoryArchiveFootage).HasColumnName("role_category_archive_footage");
              entity.Property(e => e.RoleCategoryArchiveSound).HasColumnName("role_category_archive_sound");
              entity.Property(e => e.RoleCategoryCinematographer).HasColumnName("role_category_cinematographer");
              entity.Property(e => e.RoleCategoryComposer).HasColumnName("role_category_composer");
              entity.Property(e => e.RoleCategoryDirector).HasColumnName("role_category_director");
              entity.Property(e => e.RoleCategoryEditor).HasColumnName("role_category_editor");
              entity.Property(e => e.RoleCategoryProducer).HasColumnName("role_category_producer");
              entity.Property(e => e.RoleCategoryProductionDesigner).HasColumnName("role_category_production_designer");
              entity.Property(e => e.RoleCategorySelf).HasColumnName("role_category_self");
              entity.Property(e => e.RoleCategoryWriter).HasColumnName("role_category_writer");

              entity.Property(e => e.GenreAction).HasColumnName("genre_action");
              entity.Property(e => e.GenreAdult).HasColumnName("genre_adult");
              entity.Property(e => e.GenreAdventure).HasColumnName("genre_adventure");
              entity.Property(e => e.GenreAnimation).HasColumnName("genre_animation");
              entity.Property(e => e.GenreBiography).HasColumnName("genre_biography");
              entity.Property(e => e.GenreComedy).HasColumnName("genre_comedy");
              entity.Property(e => e.GenreCrime).HasColumnName("genre_crime");
              entity.Property(e => e.GenreDocumentary).HasColumnName("genre_documentary");
              entity.Property(e => e.GenreDrama).HasColumnName("genre_drama");
              entity.Property(e => e.GenreFamily).HasColumnName("genre_family");
              entity.Property(e => e.GenreFantasy).HasColumnName("genre_fantasy");
              entity.Property(e => e.GenreFilmNoir).HasColumnName("genre_film_noir");
              entity.Property(e => e.GenreGameShow).HasColumnName("genre_game_show");
              entity.Property(e => e.GenreHistory).HasColumnName("genre_history");
              entity.Property(e => e.GenreHorror).HasColumnName("genre_horror");
              entity.Property(e => e.GenreMusic).HasColumnName("genre_music");
              entity.Property(e => e.GenreMusical).HasColumnName("genre_musical");
              entity.Property(e => e.GenreMystery).HasColumnName("genre_mystery");
              entity.Property(e => e.GenreNews).HasColumnName("genre_news");
              entity.Property(e => e.GenreRealityTv).HasColumnName("genre_reality_tv");
              entity.Property(e => e.GenreRomance).HasColumnName("genre_romance");
              entity.Property(e => e.GenreSciFi).HasColumnName("genre_sci_fi");
              entity.Property(e => e.GenreShort).HasColumnName("genre_short");
              entity.Property(e => e.GenreSport).HasColumnName("genre_sport");
              entity.Property(e => e.GenreTalkShow).HasColumnName("genre_talk_show");
              entity.Property(e => e.GenreThriller).HasColumnName("genre_thriller");
              entity.Property(e => e.GenreWar).HasColumnName("genre_war");
              entity.Property(e => e.GenreWestern).HasColumnName("genre_western");


                // entity.HasNoKey();
                // entity.ToTable("");
                // entity.Property(e => e.nconst)
                //     .HasColumnType("character varying")
                //     .HasColumnName("nconst");
                //
                // entity.Property(e => e.birthyear).HasColumnName("birthyear");
                // entity.Property(e => e.deathyear).HasColumnName("deathyear");
                // entity.Property(e => e.title_count).HasColumnName("title_count");
                // entity.Property(e => e.votes_avg).HasColumnName("votes_avg");
                // entity.Property(e => e.rating_avg).HasColumnName("rating_avg");
                // entity.Property(e => e.is_adult).HasColumnName("is_adult");
                //
                // entity.Property(e => e.release_year_avg).HasColumnName("release_year_avg");
                // entity.Property(e => e.release_year_max).HasColumnName("release_year_max");
                // entity.Property(e => e.release_year_min).HasColumnName("release_year_min");
                //
                // entity.Property(e => e.title_type_movie).HasColumnName("title_type_movie");
                // entity.Property(e => e.title_type_short).HasColumnName("title_type_short");
                // entity.Property(e => e.title_type_tv_episode).HasColumnName("title_type_tv_episode");
                // entity.Property(e => e.title_type_tv_mini_series).HasColumnName("title_type_tv_mini_series");
                // entity.Property(e => e.title_type_tv_movie).HasColumnName("title_type_tv_movie");
                // entity.Property(e => e.title_type_tv_series).HasColumnName("title_type_tv_series");
                // entity.Property(e => e.title_type_tv_short).HasColumnName("title_type_tv_short");
                // entity.Property(e => e.title_type_tv_special).HasColumnName("title_type_tv_special");
                // entity.Property(e => e.title_type_video).HasColumnName("title_type_video");
                // entity.Property(e => e.title_type_video_game).HasColumnName("title_type_video_game");
                //
                // entity.Property(e => e.role_category_actor).HasColumnName("role_category_actor");
                // entity.Property(e => e.role_category_archive_footage).HasColumnName("role_category_archive_footage");
                // entity.Property(e => e.role_category_archive_sound).HasColumnName("role_category_archive_sound");
                // entity.Property(e => e.role_category_cinematographer).HasColumnName("role_category_cinematographer");
                // entity.Property(e => e.role_category_composer).HasColumnName("role_category_composer");
                // entity.Property(e => e.role_category_director).HasColumnName("role_category_director");
                // entity.Property(e => e.role_category_editor).HasColumnName("role_category_editor");
                // entity.Property(e => e.role_category_producer).HasColumnName("role_category_producer");
                // entity.Property(e => e.role_category_production_designer).HasColumnName("role_category_production_designer");
                // entity.Property(e => e.role_category_self).HasColumnName("role_category_self");
                // entity.Property(e => e.role_category_writer).HasColumnName("role_category_writer");
                //
                // entity.Property(e => e.genre_action).HasColumnName("genre_action");
                // entity.Property(e => e.genre_adult).HasColumnName("genre_adult");
                // entity.Property(e => e.genre_adventure).HasColumnName("genre_adventure");
                // entity.Property(e => e.genre_animation).HasColumnName("genre_animation");
                // entity.Property(e => e.genre_biography).HasColumnName("genre_biography");
                // entity.Property(e => e.genre_comedy).HasColumnName("genre_comedy");
                // entity.Property(e => e.genre_crime).HasColumnName("genre_crime");
                // entity.Property(e => e.genre_documentary).HasColumnName("genre_documentary");
                // entity.Property(e => e.genre_drama).HasColumnName("genre_drama");
                // entity.Property(e => e.genre_family).HasColumnName("genre_family");
                // entity.Property(e => e.genre_fantasy).HasColumnName("genre_fantasy");
                // entity.Property(e => e.genre_film_noir).HasColumnName("genre_film_noir");
                // entity.Property(e => e.genre_game_show).HasColumnName("genre_game_show");
                // entity.Property(e => e.genre_history).HasColumnName("genre_history");
                // entity.Property(e => e.genre_horror).HasColumnName("genre_horror");
                // entity.Property(e => e.genre_music).HasColumnName("genre_music");
                // entity.Property(e => e.genre_musical).HasColumnName("genre_musical");
                // entity.Property(e => e.genre_mystery).HasColumnName("genre_mystery");
                // entity.Property(e => e.genre_news).HasColumnName("genre_news");
                // entity.Property(e => e.genre_reality_tv).HasColumnName("genre_reality_tv");
                // entity.Property(e => e.genre_romance).HasColumnName("genre_romance");
                // entity.Property(e => e.genre_scif_fi).HasColumnName("genre_scif_fi");
                // entity.Property(e => e.genre_short).HasColumnName("genre_short");
                // entity.Property(e => e.genre_sport).HasColumnName("genre_sport");
                // entity.Property(e => e.genre_talk_show).HasColumnName("genre_talk_show");
                // entity.Property(e => e.genre_thriller).HasColumnName("genre_thriller");
                // entity.Property(e => e.genre_war).HasColumnName("genre_war");
                // entity.Property(e => e.genre_western).HasColumnName("genre_western");
            });
        }

    }
}
