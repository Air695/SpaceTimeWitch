using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SpaceTimeWitch.Powers;

[RegisterPower]
public class STWPhase : ModPowerTemplate
{

    public override PowerType Type => PowerType.None;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png",
        BigIconPath: $"res://images/SpaceTimeWitch/Powers/{GetType().Name}.png"
    );
    
    // ── 白名单：不受相位限制的能力 ──
      private static readonly HashSet<Type> ApplyWhitelist = new HashSet<Type>
      {
          typeof(STWPhase)
      };

      // ── 辅助：双方相位是否匹配 ──
      private static bool SamePhase(Creature? a, Creature? b)
      {
          if (a == null || b == null)
              return true;
          return a.HasPower<STWPhase>() == b.HasPower<STWPhase>();
      }

      // ═══════════════════════════════════════════
      // 0. 回合开始时移除（仅常规回合，跳过额外回合）
      // ═══════════════════════════════════════════

      public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext,
          CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
      {
          if (side == CombatSide.Player
              && CombatManager.Instance.PlayersTakingExtraTurn.Count == 0)
              await PowerCmd.Remove(this);
      }

      // ═══════════════════════════════════════════
      // 1. 目标选择
      // ═══════════════════════════════════════════

      public override bool ShouldAllowTargeting(Creature target)
      {
          if (target == Owner)
              return true;
          return target.HasPower<STWPhase>();
      }

      // ShouldAllowHitting 被 CanReceivePowers 复用，不能在此拦截相位过滤，
      // 否则已有相位者会阻止无相位者接受任何能力（包括相位本身）。
      // 相位过滤由 ModifyDamageMultiplicative / ModifyBlockMultiplicative /
      // TryModifyPowerAmountReceived 负责。
      public override bool ShouldAllowHitting(Creature creature) => true;

      // ═══════════════════════════════════════════
      // 2. 伤害
      // ═══════════════════════════════════════════

      public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount,
          ValueProp props, Creature? dealer, CardModel? cardSource)
      {
          if (dealer == null || target == null)
              return 1m;
          if (Owner != dealer && Owner != target)
              return 1m;
          return SamePhase(dealer, target) ? 1m : 0m;
      }

      // ═══════════════════════════════════════════
      // 3. 格挡
      // ═══════════════════════════════════════════

      public override decimal ModifyBlockMultiplicative(Creature target, decimal block,
          ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
      {
          var sourceCreature = cardSource?.Owner?.Creature;
          if (sourceCreature == null)
              return 1m;
          if (Owner != sourceCreature && Owner != target)
              return 1m;
          return SamePhase(sourceCreature, target) ? 1m : 0m;
      }

      // ═══════════════════════════════════════════
      // 4. 能力施加
      // ═══════════════════════════════════════════

      public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower,
          Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
      {
          if (applier == null)
          {
              modifiedAmount = amount;
              return false;
          }
          if (Owner != applier && Owner != target)
          {
              modifiedAmount = amount;
              return false;
          }
          // 白名单能力不受相位匹配限制
          if (canonicalPower is STWPhase || ApplyWhitelist.Contains(canonicalPower.GetType()))
          {
              modifiedAmount = amount;
              return false;
          }
          if (!SamePhase(applier, target))
          {
              modifiedAmount = 0m;
              return true;
          }
          modifiedAmount = amount;
          return false;
      }

      // ═══════════════════════════════════════════
      // 5. 卡牌操纵
      // ═══════════════════════════════════════════

      public override bool ShouldAddToDeck(CardModel card)
      {
          if (Owner.IsPlayer
              && card.Owner == Owner.Player
              && (card.Type == CardType.Status || card.Type == CardType.Curse))
              return false;
          return true;
      }

      public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
      {
          newCard = null;
          if (Owner.IsPlayer
              && card.Owner == Owner.Player
              && (card.Type == CardType.Status || card.Type == CardType.Curse))
              return true;
          return false;
      }

      public override bool TryModifyCardBeingAddedToDeckLate(CardModel card, out CardModel? newCard)
      {
          newCard = null;
          if (Owner.IsPlayer
              && card.Owner == Owner.Player
              && (card.Type == CardType.Status || card.Type == CardType.Curse))
              return true;
          return false;
      }

      public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
      {
          if (card.Type != CardType.Status && card.Type != CardType.Curse)
              return;
          if (card.Owner.Creature != Owner)
              return;
          if (creator == null)
          {
              Flash();
              await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
          }
      }
  }