using HumanResources.Utilities;
using System;
using System.Text;
using Tweetinvi.Models;

namespace HumanResources.TwitterModule
{
  public static class TwitterUserExtensions
  {
    public static double DaysAlive(this IUser user)
    {
      return (DateTime.Now - user.CreatedAt).TotalDays;
    }

    public static double FavsPerDay(this IUser user)
    {
      return user.FavouritesCount / user.DaysAlive();
    }

    public static double TweetsPerDay(this IUser user)
    {
      return user.StatusesCount / user.DaysAlive();
    }

    public static double Ratio(this IUser user)
    {
      if (user.FollowersCount == 0)
      {
        return 0.0;
      }
      else if (user.FriendsCount == 0)
      {
        return double.PositiveInfinity;
      }
      return (double)user.FollowersCount / user.FriendsCount;
    }

    public static string FormattedAge(this IUser user)
    {
      var sb = new StringBuilder();
      var age = LogUtil.CalculateAge(user.CreatedAt);
      sb.AppendFormat("{0}y{1}m{2}d {3}h{4}m{5}s", age.Item1, age.Item2, age.Item3, age.Item4, age.Item5, age.Item6);
      return sb.ToString();
    }

    public static double Score(this IUser user)
    {
      var total = 0.0;
      double dayScore = Math.Pow(user.DaysAlive(), 1.05);
      total += dayScore;

      double favAbs = Math.Abs(user.FavsPerDay() - 7);
      if (favAbs < 0.1)
      {
        favAbs = 0.1;
      }
      double favScore = user.FavouritesCount / 7 / Math.Sqrt(favAbs * 7);
      total += favScore;

      double tweetAbs = Math.Abs(user.TweetsPerDay() - 5);
      if (tweetAbs < 0.1)
      {
        tweetAbs = 0.1;
      }
      double tweetScore = user.StatusesCount / Math.Sqrt(tweetAbs * 5);
      total += tweetScore;

      double frndScore = Math.Log(user.FriendsCount == 0 ? 1 : user.FriendsCount);
      double follScore = Math.Pow(user.FollowersCount == 0 ? 1 : user.FollowersCount, 1.01);
      double ratio = user.Ratio();
      if (ratio == 0)
      {
        ratio = 1.0;
      }
      else if (double.IsPositiveInfinity(ratio))
      {
        ratio = user.FollowersCount;
      }
      double ratScore = (frndScore + follScore) * Math.Log(ratio);
      total += ratScore;

      double finalCoeff = 1.0;
      finalCoeff *= user.Protected ? 0.9 : 1.0;
      finalCoeff *= user.Verified ? 2.0 : 1.0;

      double defCoeff = 1.0;
      if (user.DefaultProfile && user.DefaultProfileImage)
      {
        defCoeff = 0.5;
      }
      else if (user.DefaultProfileImage)
      {
        defCoeff = 0.8;
      }
      else if (user.DefaultProfile)
      {
        defCoeff = 0.95;
      }
      finalCoeff *= defCoeff;
      finalCoeff *= !string.IsNullOrEmpty(user.Location) ? 1.05 : 1.0;

      return total * finalCoeff;
    }
  }
}
