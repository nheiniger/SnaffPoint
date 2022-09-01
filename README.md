# SnaffPoint

## What is it
SnaffPoint is a tool for pointesters who are in need of some sweetness in this world. It should help you find sensitive files available on SharePoint online and on shared OneDrive files for your company (or your customer).

## How does it work
There are actually 2 tools:
- GetBearerToken will perform authentication for you on SharePoint, with the usual GUI and supporting MFA
- SnaffPoint is CLI only and will do the enumeration itself and find the interesting files

### GetBearerToken
Nothing fancy, run the tool as follows:
```
GetBearerToken.exe https://yoururl.sharepoint.com
```
Authenticate successfully and you should get a Bearer token to use in SnaffPoint. This is mostly the code of PNP-Tools (see credits below).

### SnaffPoint
Have a look at the help menu here:
```
Usage: SnaffPoint.exe -u URL -t JWT [OPTIONS]

-h, --help              This is me :)

Mandatory:
-u, --url               SharePoint online URL where you want to search
-t, --token             Bearer token that grants access to said SharePoint

Common options:
-m, --max-rows          Max. number of rows to return per search query (default is 50)

Presets mode (default):
-p, --preset            Path to a folder containing XML search presets (default is ./presets)

Single query mode:
-q, --query             Query search string
-l, --fql               Enables FQL (default is KQL)
-r, --refinement-filter Adds a refinement filter
```

Note that the `preset` folder contains many presets that you may or may not want to test on your environment. Have a look at them, and please submit a pull request if you have ideas for other presets. The preset mode is probably what you want to run against your own company. On the other hand, if you need to run the C# assembly in memory, you probably want to specify a single query to avoid the presets on disk and have more control on what you search.

## Due credits
@mikeloss and @sh3r4_hax for Snaffler (https://github.com/SnaffCon/Snaffler) from which I borrowed the name and adapted many of the rules.

The contibutors of PNP-Tools, all the code interacting with SharePoint is ~~ripped off~~ inspired by their Search.QueryTool project: https://github.com/pnp/PnP-Tools/tree/master/Solutions/SharePoint.Search.QueryTool/SearchQueryTool
