using UnityEngine;

public interface IInteractable 
{


    public bool TryInteract(PlayerBehaviour player);

    public bool TryInteractAlternate(PlayerBehaviour player);

}
