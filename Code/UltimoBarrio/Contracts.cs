using Sandbox;
using System;

namespace UltimoBarrio
{
    public interface IInventoryOwner
    {
        bool CanAdd(string itemId, int amount);
        bool TryAdd(string itemId, int amount);
        bool TryRemove(string itemId, int amount);
        int GetCount(string itemId);
    }

    public interface IDamageable
    {
        void TakeDamage(float amount, Vector3 position, Vector3 force, Guid attackerId);
        float Health { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
    }

    public interface IInteractable
    {
        string GetInteractionPrompt();
        bool CanInteract(Guid playerId);
        void OnInteract(Guid playerId);
    }

    public interface IApartmentAccessPolicy
    {
        bool CanEnter(Guid apartmentId, Guid playerId);
        bool CanAccessStash(Guid apartmentId, Guid playerId);
    }

    public interface IWorldClock
    {
        float CurrentTimeOfDay { get; }
        string CurrentPhase { get; }
        float PhaseTimeRemaining { get; }
        event Action<string> OnPhaseChanged;
    }

    public interface IRaidParticipant
    {
        void StartRaid();
        void EndRaid();
        bool IsActiveInRaid { get; }
    }
}
