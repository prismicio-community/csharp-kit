## Migration from the old .NET development kit

The logic stays the same, so by following compiler error migrating is fairly easy.
You will end up with a much simplier code.

Here is a list of changes:

* The `prismic.extensions` namespace is gone, most of the classes are on the `prismic` namespace
* `FSharpOption` are no longer used, `null` is used instead
* It is no longer necessary to call `SubmitableAsTask()` before `Submit()`
* To be consistent with other dotnet classes, attributes should now by used with the capitalized accessor (e.g. `.Slug` instead of `.slug`)
* Instead of getting a form with `Api.Forms["everything"]`, you need to use `Api.Form("everything")`
* The Kit now ships with an in-memory LRU Cache, it is recommended to use it rather than implementing your own. If you do want to implement your own, just implement the `ICache` interface.
* `LinkResolver.Resolve` instead of `LinkResolver.Apply`
