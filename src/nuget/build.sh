#!/bin/bash
if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
  mono ./NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi
export FSHARPI=`which fsharpi`
cat - > fsharpi <<"EOF"
#!/bin/bash
libdir=$PWD/packages/FAKE/tools/
$FSHARPI --lib:$libdir $@
EOF
chmod +x fsharpi
mono packages/FAKE/tools/FAKE.exe build.fsx $@
rm fsharpi