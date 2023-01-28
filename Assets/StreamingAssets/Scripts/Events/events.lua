events = {}

events.begin_generic = {
	title = "#EVENT_BEGIN_GENERIC_TITLE", -- Event Title
	desc = "#EVENT_BEGIN_GENERIC_DESC", -- Event Description
	trigger_once = true, -- Should this be called and removed from the event pool for the remainder of the game?
	trigger = function() return true end, -- The weekly check for triggering the event.
	effects = function() -- Called once triggered
		local button = {"#BUTTON_GENERIC_CONFIRM", function() end} -- Button structure. Does nothing.
		event_window("begin_generic", button) -- Makes a popup window for the event.
	end
}

return events