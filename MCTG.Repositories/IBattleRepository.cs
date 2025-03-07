using MCTG.Models;

namespace MCTG.Repositories
{
    public interface IBattleRepository
    {
        Task<Battle> CreateBattle(Battle battle);
        Task UpdateBattleResult(Battle battle);
    }
}