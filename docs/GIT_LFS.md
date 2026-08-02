# Git LFS

No se ha activado Git LFS automáticamente para evitar bloquear el primer commit.

Actívalo antes de añadir fuentes binarias pesadas propias:

```bash
git lfs install
git lfs track "*.fbx"
git lfs track "*.blend"
git lfs track "*.wav"
git lfs track "*.psd"
git add .gitattributes
git commit -m "chore: configure git lfs"
```

No copies assets remotos a Git solo para evitar una dependencia. Prefiere referencias de s&box cuando la licencia y disponibilidad lo permitan.
