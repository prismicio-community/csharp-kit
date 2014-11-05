#!/bin/sh -x

NUNIT=2.6.3

mono --runtime=v4.0 src/nuget/NuGet.exe install NUnit.Runners -Version $NUNIT -o src/packages

runTest(){
    mono --runtime=v4.0 src/packages/NUnit.Runners.${NUNIT}/tools/nunit-console.exe -noxml -nodots -labels -stoponerror $@
   if [ $? -ne 0 ]
   then   
     exit 1
   fi
}

#This is the call that runs the tests and adds tweakable arguments.
#In this case I'm excluding tests I categorized for performance.
runTest $1 -exclude=Performance

exit $?

