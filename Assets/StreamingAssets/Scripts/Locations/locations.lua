locations = {}

locations.siberia = {
    name = "#LOCATION_SIBERIA_NAME",
    desc = "#LOCATION_SIBERIA_DESC",
    start_effect = function ()
        local USCI = world.factions["USCI"]
        local player = world.factions["PLAYER"]

        USCI.relations.Add("")
    end
}

locations.far = {
    name = "#LOCATION_FAR_NAME",
    desc = "#LOCATION_FAR_DESC",
}

locations.oceania = {
    name = "#LOCATION_OCEANIA_NAME",
    desc = "#LOCATION_OCEANIA_DESC",
}

locations.swahili = {
    name = "#LOCATION_SWAHILI_NAME",
    desc = "#LOCATION_SWAHILI_DESC",
}

locations.iberia = {
    name = "#LOCATION_IBERIA_NAME",
    desc = "#LOCATION_IBERIA_DESC",
}

return locations