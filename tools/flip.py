import json

with open(r"D:\Document\anake_card\tools\cards_data.json",encoding="utf-8") as f:
    d = json.load(f)
d_cp = []
for i in d:
    at = i["attack"]
    lif = i["life"]
    i["attack"] = lif
    i["life"] = at
    d_cp.append(i)

with open(r"D:\Document\anake_card\tools\cards_data_flip.json", "w", encoding="utf-8") as f:
    json.dump(d_cp, f, ensure_ascii=False, indent=4)