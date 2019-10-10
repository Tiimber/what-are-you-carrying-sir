using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class MeshAndPositionToRandomize {

    public AssignMeshAndPositionToObject[] objectsToSetMeshAndPosition;
    public MeshAndPosition[] meshesAndPositions;

    public void assign() {
        MeshAndPosition randomMeshAndPosition = ItsRandom.pickRandom(meshesAndPositions.ToList());
        foreach(AssignMeshAndPositionToObject objectToSetMeshAndPosition in objectsToSetMeshAndPosition) {
            objectToSetMeshAndPosition.mesh = randomMeshAndPosition.mesh;
            objectToSetMeshAndPosition.position = randomMeshAndPosition.position;
        }
    }
}
