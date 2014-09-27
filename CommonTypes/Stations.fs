namespace CommonTypes

type station = { name: string; serving: string array; point: (decimal * decimal); }
type rmstationkey = { station: station; key: string;}   
