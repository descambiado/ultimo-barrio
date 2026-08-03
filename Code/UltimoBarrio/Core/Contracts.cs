// SPDX-License-Identifier: MPL-2.0
using Sandbox;
using System;
using System.Collections.Generic;
using UltimoBarrio.Persistence;

namespace UltimoBarrio.Core;

public enum GamePhase
{
    Day,
    Preparation,
    Night,
    Aftermath
}

public struct DamageEvent
{
    public float Amount;
    public Vector3 Position;
    public Vector3 Force;
    public string AttackerId;
    public string WeaponId;
}

public struct InteractionRequest
{
    public string InteractorId;
    public GameObject InteractorObject;
}

public interface IWorldInteractable
{
    string GetInteractionPrompt(InteractionRequest request);
    bool CanInteract(InteractionRequest request);
    void OnInteract(InteractionRequest request);
}

public interface IWorldContainer
{
    InventoryComponent GetContainerInventory();
}

public interface IInteractable : IWorldInteractable
{
}

public interface IPlayerIdentityProvider
{
    bool TryResolve(Connection connection, out string playerId);
}

public interface IInventory
{
    string InventoryId { get; }
    int MaxSlots { get; }
    bool CanAdd(string itemId, int amount);
    bool TryAdd(string itemId, int amount);
    bool TryRemove(string itemId, int amount);
    int GetCount(string itemId);
}

public interface IInventoryContainer
{
    IInventory GetInventory();
}

public interface IApartmentAccessPolicy
{
    bool CanEnter(string apartmentId, string playerId);
    bool CanAccessStash(string apartmentId, string playerId);
}

// Removed IPersistenceProvider as it lives in UltimoBarrio.Persistence

public interface IWallet
{
    string WalletId { get; }
    int Balance { get; }
    bool CanAfford(int amount);
    bool TryWithdraw(int amount);
    void Deposit(int amount);
}

public interface IDamageable
{
    float Health { get; }
    float MaxHealth { get; }
    bool IsDead { get; }
    void TakeDamage(DamageEvent damageEvent);
}

public interface IWorldClock
{
    float CurrentTimeOfDay { get; }
    GamePhase CurrentPhase { get; }
    float PhaseTimeRemaining { get; }
    event Action<GamePhase> OnPhaseChanged;
    
    void ForcePhase(GamePhase phase);
}

public interface IRaidTarget
{
    string TargetId { get; }
    Vector3 TargetPosition { get; }
    float Priority { get; }
    void OnRaidStarted();
    void OnRaidEnded(bool success);
}
