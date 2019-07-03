# BACCKER-UPPER
Simple backup/archiving CLI for performing differential backups targeting ZIP archives

## Example configuration file
```js
{

  // Sources
  "sources": [

    {
			// Separately backup every directory inside this path
      "path": "c:/myprojects/*",
			// Exclude paths that match following
      "exclude": [
        ".bak"
        "/logs",
      ]
    }

  ]

}
```