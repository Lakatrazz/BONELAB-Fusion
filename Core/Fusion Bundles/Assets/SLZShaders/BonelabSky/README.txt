# Sky Prefab For Bonelab

This package contains a prefab for rendering sky in Bonelab. Unity's default
sky rendering is turned off due to it being incompatible with the fog and vol-
umetric systems we use. The included prefab can be used as a replacement for
the default sky. The prefab is simply an enormous mesh with no occlusion and
a special shader. Note that the included shader (SLZ/Sky With Fog) must only be
used with the included mesh (Sky_Mesh) as the shader expects a single triangle
mesh. Additionally, Sky_Mesh has a 65x65x65 kilometer bounding box to ensure it
is always rendered. The included prefab further has dynamic occlusion disabled
to ensure it never gets culled, as well as all unnecessary lighting information
disabled. Do not enable the Occluder or Occludee Static flags on the prefab to
ensure the mesh is never occluded.
