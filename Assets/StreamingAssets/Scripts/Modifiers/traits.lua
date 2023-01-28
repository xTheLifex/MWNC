traits = {}

-- Legendary Leader Trait
traits.legendary_leader = {
    name = "#TRAIT_LEGENDARY_LEADER",
    scope = {"LEADER"},
    effects = {
        disengage_chance = 15, -- +15% Disengagement Chance
        unity = 30, -- +30% Troop unity
    },
    canAdd = function(pers) -- Cannot be applied to Training personnel
        if (pers.subtype == "TRAINING") then return false end
        return true
    end
}

traits.legendary_instructor = {
    name = "#TRAIT_LEGENDARY_INSTRUCTOR",
    scope = {"LEADER"},
    effects = {
        train_time = -25, -- -25% Training time
        accident_chance = -25, -- -25% Chance of accidents
        train_lowestrank_chance = -100 -- -100% Chance of a D rank unit
        train_lowrank_chance = -20 -- -20% Chance of a C rank unit
        train_medrank_chance = 5 -- +5% Chance of a B rank unit
        train_highrank_chance = 15 -- +15% Chance of a A rank unit
        train_equipment_use = 50 -- +50% Equipment consumption.
    },
    canAdd = function (pers)
        return (pers.subtype == "TRAINING") -- Only training personnel can have this trait.
    end
}

traits.legendary_soldier = {
    name = "#TRAIT_LEGENDARY_SOLDIER",
    scope = {"COMBATANT"},
    effects = {
        evasion = 15,
        damage = 15,
        speed = 15,
    },
    canAdd = function (pers) -- Only personnel of type A or B can be Legendary.
        if (pers.subtype == "A" or pers.subtype == "B") then
            return true
        end
        return false
    end
}

traits.incompetent = {
    name = "#TRAIT_INCOMPENTENT_SOLDIER",
    scope = {"COMBATANT"},
    effects = {}, -- No modifier effects directly.
    canAdd = function (pers) -- Only of type C or D.
        if (pers.subtype == "C" or pers.subtype == "D") then
            return true
        end
        return false
    end,
    process = function (world, pers) -- Called every week
        
        local chance = math.random(0,100)
        if (chance > 90) then
            
        end


        local ally = pers.unit.GetRandomAlly()
        ally:Damage(0.25)

        disregard = {"#EVENT_DYNAMIC_ACCIDENT_DISREGARD", function ()
            
        end}

        fire = {"#EVENT_DYNAMIC_ACCIDENT_FIRE", function ()
            pers.unit.Remove(pers.id)
        end, L("#EVENT_DYNAMIC_ACCIDENT_FIRE_DESC", pers.unit.name, pers.name)} -- e.g: "John Doe is removed from 7th Division"

        local id = math.random(1,3)
        local title = "#EVENT_DYNAMIC_ACCIDENT_NAME" .. id
        local desc = "#EVENT_DYNAMIC_ACCIDENT_DESC" .. id
        window(title, desc, disregard, fire)
    end

}

return traits