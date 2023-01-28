relations = {}

relations.starting_siberia = {
    name = "#RELATIONS_STARTING_SIBERIA"
    value = 25,
    onAdd = function ()
        -- Add stockpile of USCI and Russian equipment to player
        local player = world.factions["PLAYER"]
        if (player) then
            player.AddEquipment("USCI_CAR") -- TODO: Test equipment. Change later.
            player.AddEquipment("RUS_CAR")
        end
    end,
    onRemove = function () end,
    duration = -1, -- Infinite
}


return relations