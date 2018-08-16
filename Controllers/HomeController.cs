using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GamesDatabase_Project.Models;

namespace GamesDatabase_Project.Controllers
{
    public class HomeController : Controller
    {
        GameReleaseDateDatabaseEntities6 db = new GameReleaseDateDatabaseEntities6();

        public ActionResult Index(string searchString, string gameGenre, string gamePlatform)
        {
            List<string> genreList = new List<string>();
            var genreQuery = from gen in db.genres
                             orderby gen.Genre1
                             select gen.Genre1;
            genreList.AddRange(genreQuery.Distinct());
            ViewBag.gameGenre = new SelectList(genreList);

            List<string> platformList = new List<string>();
            var platformQuery = from plat in db.platforms
                                select plat.Platform1;
            platformList.AddRange(platformQuery.Distinct());
            ViewBag.gamePlatform = new SelectList(platformList);

            var games = from g in db.Games
                        select g;

            //filter by game genre
            if (!String.IsNullOrEmpty(gameGenre))
            {
                var myGenreID = db.genres.FirstOrDefault(i => i.Genre1 == gameGenre);

                var myGenreTable = from g in db.gameGenres
                                   where g.genreID == myGenreID.ID
                                   select g;

                games = from g in games
                        where myGenreTable.Any(f => f.gameID == g.ID)
                        select g;
            }

            //filter by game platform
            if (!String.IsNullOrEmpty(gamePlatform))
            {
                //get corresponding ID from platform (e.g. ps4 = 1, xbox = 2)
                var myPlatformID = db.platforms.FirstOrDefault(i => i.Platform1 == gamePlatform);

                //filter controller table by platform ID
                var myPlatformTable = from g in db.gamePlatforms
                                      where g.platformID == myPlatformID.ID
                                      select g;

                //filter game table by controller table ID
                games = from g in games
                        where myPlatformTable.Any(f => f.gameID == g.ID)

                        select g;
            }

            //filter by name
            if (!String.IsNullOrEmpty(searchString))
            {
                games = games.Where(x => x.Title.Contains(searchString));
            }

            return View(games);
        }

        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Game game, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                db.Games.Add(game);

                if (!String.IsNullOrEmpty(collection["Genre(s)"]))
                {
                    string[] userGenreString;
                    if (collection["Genre(s)"].Contains(','))
                    {
                        userGenreString = collection["Genre(s)"].Split(',');
                    }
                    else
                    {
                        userGenreString = new string[1];
                        userGenreString[0] = collection["Genre(s)"];
                    }


                    foreach (string item in userGenreString)
                    {
                        String myItem = item;
                        //System.Diagnostics.Debug.WriteLine(myItem);
                        if (!String.IsNullOrEmpty(myItem))
                        {
                            if (myItem[0] == ' ')
                            {
                                //remove inital spacing after comma (e.g. "Action, Adventure)
                                myItem = myItem.Substring(1);
                            }

                            var myGenreID = db.genres.FirstOrDefault(i => i.Genre1.ToLower() == myItem.ToLower());

                            if (myGenreID != null) //if genre is valid
                            {
                                gameGenre newGameGenre = new gameGenre();
                                newGameGenre.gameID = game.ID;
                                newGameGenre.genreID = myGenreID.ID;
                                db.gameGenres.Add(newGameGenre);

                                //System.Diagnostics.Debug.WriteLine("Added " + myItem);
                            }
                        }



                    }


                }


                if (!String.IsNullOrEmpty(collection["Platform(s)"]))
                {
                    string[] userPlatformString;
                    if (collection["Platform(s)"].Contains(','))
                    {
                        userPlatformString = collection["Platform(s)"].Split(',');
                    }
                    else
                    {
                        userPlatformString = new string[1];
                        userPlatformString[0] = collection["Platform(s)"];
                    }

                    foreach (string item in userPlatformString)
                    {
                        String myItem = item;
                        //System.Diagnostics.Debug.WriteLine(myItem);
                        if (!String.IsNullOrEmpty(myItem))
                        {
                            if (myItem[0] == ' ')
                            {
                                //remove inital spacing after comma (e.g. "Steam, Switch)
                                myItem = myItem.Substring(1);
                            }

                            switch (myItem.ToLower())
                            {
                                case "ps4":
                                case "playstation4":
                                case "playstation-4":
                                case "playstation_4":
                                case "playstation 4":
                                    myItem = "Playstation 4";
                                    break;

                                case "xb1":
                                case "xbox1":
                                case "xbone":
                                case "xbox-one":
                                case "xboxone":
                                case "xbox_one":
                                case "xbox one":
                                    myItem = "Xbox One";
                                    break;

                                case "nintendo switch":
                                case "switch":
                                    myItem = "Switch";
                                    break;

                                case "pc":
                                case "steam":
                                    myItem = "Steam";
                                    break;

                                default:
                                    myItem = null;
                                    break;
                            }

                            if (myItem != null)
                            {
                                var myPlatformID = db.platforms.FirstOrDefault(i => i.Platform1 == myItem);

                                gamePlatform newGamePlatform = new gamePlatform();
                                newGamePlatform.gameID = game.ID;
                                newGamePlatform.platformID = myPlatformID.ID;
                                db.gamePlatforms.Add(newGamePlatform);
                                //System.Diagnostics.Debug.WriteLine("Added");
                            }
                        }
                    }
                }


                db.SaveChanges();
                return RedirectToAction("index");
            }
            return View(game);
        }

        public ActionResult Edit(int id)
        {
            Game game = db.Games.Find(id);

            var myGenreTable = from g in db.gameGenres
                               where g.genreID == game.ID
                               select g;
            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Game game, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                db.Entry(game).State = System.Data.Entity.EntityState.Modified;

                string[] userGenreTable = convertGenreString(collection["Genre(s)"]);
                string[] userPlatformTable = convertPlatformString(collection["Platform(s)"]);

                var DBgenreTable = from g in db.gameGenres
                                   where g.gameID == game.ID
                                   select g;

                var DBplatformTable = from g in db.gamePlatforms
                                      where g.gameID == game.ID
                                      select g;

                foreach (var genre in db.genres)
                {
                    bool onUserString = false;
                    bool onDBgenreTable = false;

                    genre myGenreID;

                    foreach (string item in userGenreTable)
                    {
                        myGenreID = db.genres.FirstOrDefault(i => i.Genre1 == item);

                        if (myGenreID != null) //if genre is valid
                        {
                            if (genre.ID == myGenreID.ID)
                            {
                                onUserString = true;
                                break;
                            }
                        }
                    }

                    foreach (var gameGenre in DBgenreTable)
                    {
                        if (genre.ID == gameGenre.genreID)
                        {
                            onDBgenreTable = true;
                            break;
                        }
                    }

                    if (onUserString)
                    {
                        //user wants to add this genre
                        if (onDBgenreTable)
                        {
                            //its already on the table -> do nothing
                        }
                        else
                        {
                            //not yet on table -> add genre
                            gameGenre newGameGenre = new gameGenre();
                            newGameGenre.gameID = game.ID;
                            newGameGenre.genreID = genre.ID;
                            db.gameGenres.Add(newGameGenre);
                            //System.Diagnostics.Debug.WriteLine("Added");
                        }
                    }
                    else if (onDBgenreTable)
                    {
                        //on table but user has removed it from string -> remove genre
                        db.gameGenres.RemoveRange(db.gameGenres.Where(g => g.gameID == game.ID && g.genreID == genre.ID));
                    }
                }


                foreach (var platform in db.platforms)
                {
                    bool onUserString = false;
                    bool onDBplatformTable = false;

                    platform myPlatformID;
                    foreach (string item in userPlatformTable)
                    {
                        myPlatformID = db.platforms.FirstOrDefault(i => i.Platform1 == item);

                        if (myPlatformID != null)
                        {
                            if (platform.ID == myPlatformID.ID)
                            {
                                onUserString = true;
                                break;
                            }
                        }
                    }

                    foreach (var gamePlatform in DBplatformTable)
                    {
                        if (platform.ID == gamePlatform.platformID)
                        {
                            onDBplatformTable = true;
                            break;
                        }
                    }

                    if (onUserString)
                    {
                        //user wants to add this platform
                        if (onDBplatformTable)
                        {
                            //Its already on the table -> do nothing
                        }
                        else
                        {
                            //Not yet on table -> add platform
                            gamePlatform newGamePlatform = new gamePlatform();
                            newGamePlatform.gameID = game.ID;
                            newGamePlatform.platformID = platform.ID;
                            db.gamePlatforms.Add(newGamePlatform);
                        }
                    }
                    else if (onDBplatformTable)
                    {
                        //on table but user has removed it from string -> remove platform
                        db.gamePlatforms.RemoveRange(db.gamePlatforms.Where(g => g.gameID == game.ID && g.platformID == platform.ID));
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(game);
        }

        private string[] convertGenreString(string collection)
        {
            string[] userGenreString;
            if (collection.Contains(','))
            {
                userGenreString = collection.Split(',');

            }
            else
            {
                userGenreString = new string[1];
                userGenreString[0] = collection;
            }

            for (int i = 0; i < userGenreString.Length; i++)
            {
                userGenreString[i] = formatUserString(userGenreString[i]);
            }

            return userGenreString;
        }

        private string[] convertPlatformString(string collection)
        {
            string[] userPlatformString;
            if (collection.Contains(','))
            {
                userPlatformString = collection.Split(',');
            }
            else
            {
                userPlatformString = new string[1];
                userPlatformString[0] = collection;
            }

            for (int i = 0; i < userPlatformString.Length; i++)
            {
                userPlatformString[i] = formatUserString(userPlatformString[i]);

                switch (userPlatformString[i].ToLower())
                {
                    case "ps4":
                    case "playstation4":
                    case "playstation-4":
                    case "playstation_4":
                    case "playstation 4":
                        userPlatformString[i] = "Playstation 4";
                        break;

                    case "xb1":
                    case "xbox1":
                    case "xbone":
                    case "xbox-one":
                    case "xboxone":
                    case "xbox_one":
                    case "xbox one":
                        userPlatformString[i] = "Xbox One";
                        break;

                    case "nintendo switch":
                    case "switch":
                        userPlatformString[i] = "Switch";
                        break;

                    case "pc":
                    case "steam":
                        userPlatformString[i] = "Steam";
                        break;

                    default:
                        userPlatformString[i] = null;
                        break;
                }
                //System.Diagnostics.Debug.WriteLine(userPlatformString[i]);
            }

            return userPlatformString;
        }

        private string formatUserString(string item)
        {
            String tempString = item;
            if (tempString[0] == ' ')
            {
                //remove inital spacing after comma (e.g. "Action, Adventure)
                tempString = tempString.Substring(1);
            }
            if (tempString[tempString.Length - 1] == ' ' ||
                tempString[tempString.Length - 1] == '.' ||
                tempString[tempString.Length - 1] == ',')
            {
                tempString = tempString.Remove(tempString.Length - 1);
            }

            item = tempString.First().ToString().ToUpper() + tempString.Substring(1).ToLower();


            return item;
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Game game = db.Games.Find(id);

            if (game == null)
            {
                return HttpNotFound();
            }

            return View(game);
        }

        public ActionResult Delete(int id)
        {
            Game game = db.Games.Find(id);
            return View(game);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Game game = db.Games.Find(id);

            db.gameGenres.RemoveRange(db.gameGenres.Where(g => g.gameID == game.ID));
            db.gamePlatforms.RemoveRange(db.gamePlatforms.Where(g => g.gameID == game.ID));
            db.Games.Remove(game);

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public int? LikeAction(int id)
        {
            System.Diagnostics.Debug.WriteLine("test");
            var LikedGame = db.Games.FirstOrDefault(g => g.ID == id);

            if (LikedGame != null)
            {
                LikedGame.Upvote++;
                db.SaveChanges();
            }

            return LikedGame.Upvote;
        }

        public int? DislikeAction(int id)
        {
            var LikedGame = db.Games.FirstOrDefault(g => g.ID == id);

            if (LikedGame != null)
            {
                LikedGame.Upvote--;
                db.SaveChanges();
            }

            return LikedGame.Upvote;
        }
    }
}
