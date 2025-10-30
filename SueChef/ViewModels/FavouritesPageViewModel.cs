namespace SueChef.ViewModels;

using Microsoft.AspNetCore.SignalR;
using SueChef.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FavouritesPageViewModel
{
    public IEnumerable<FavouritesViewModel>? Favourites { get; set; } = new List<FavouritesViewModel>();
    public FavouritesViewModel? FavouritesViewModel { get; set; }
    public RecipeCardViewModel? RecipeCardViewModel { get; set; }
}