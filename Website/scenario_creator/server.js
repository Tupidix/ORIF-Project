const express = require('express');
const bodyParser = require('body-parser');
const path = require('path');
const { Sequelize, DataTypes } = require('sequelize');

// Express
const app = express();
const PORT = process.env.PORT || 5000;

// SQLite
const sequelize = new Sequelize({
    dialect: 'sqlite',
    storage: 'scenarios.db',
    logging: true
});

// Models
const Scenario = sequelize.define('Scenario', {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    name: { type: DataTypes.STRING, allowNull: false },
    description: { type: DataTypes.STRING, allowNull: false },
    mode: { type: DataTypes.STRING, allowNull: false },
    level: { type: DataTypes.STRING, allowNull: false },
    allowed_time: { type: DataTypes.FLOAT, allowNull: false },
    validator_position: { type: DataTypes.BOOLEAN, defaultValue: false },
    validator_rotation: { type: DataTypes.BOOLEAN, defaultValue: false },
    validator_order: { type: DataTypes.BOOLEAN, defaultValue: false }
});

const Menu = sequelize.define('Menu', {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    name: { 
        type: DataTypes.STRING, 
        allowNull: false,
        validate: { notEmpty: true } 
    },
    description: { type: DataTypes.STRING, allowNull: true },
    appetizer: { type: DataTypes.STRING, allowNull: true },
    mainCourse: { type: DataTypes.STRING, allowNull: true },
    dessert: { type: DataTypes.STRING, allowNull: true }
});

const Cutlery = sequelize.define('Cutlery', {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    tag: { type: DataTypes.STRING, allowNull: false },
    text: { type: DataTypes.STRING, allowNull: false }
});

const MenuCutleries = sequelize.define('MenuCutleries', {
    parent: { type: DataTypes.STRING, allowNull: false },
    placementOrderIndex: { type: DataTypes.INTEGER, allowNull: false },
    offset: { type: DataTypes.JSON, allowNull: false }
});

const Drink = sequelize.define('Drink', {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    type: { type: DataTypes.STRING, allowNull: false }
});

const Extra = sequelize.define('Extra', {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    tag: { type: DataTypes.STRING, allowNull: false },
    name: { type: DataTypes.STRING, allowNull: false }
});

const ScenarioDrinks = sequelize.define('ScenarioDrinks', {
    id: { type: DataTypes.INTEGER, primaryKey: true, autoIncrement: true },
    name: { type: DataTypes.STRING, allowNull: true }  
});

const ScenarioMenus = sequelize.define('ScenarioMenus', {
    id: { 
        type: DataTypes.INTEGER, 
        primaryKey: true, 
        autoIncrement: true 
    },
    ScenarioId: { 
        type: DataTypes.INTEGER, 
        allowNull: false, 
        references: { model: 'Scenarios', key: 'id' }, 
        onDelete: 'CASCADE' 
    },
    MenuId: { 
        type: DataTypes.INTEGER, 
        allowNull: false, 
        references: { model: 'Menus', key: 'id' }, 
        onDelete: 'CASCADE' 
    },
    createdAt: {
        type: DataTypes.DATE,
        allowNull: false,
        defaultValue: Sequelize.NOW
    },
    updatedAt: {
        type: DataTypes.DATE,
        allowNull: false,
        defaultValue: Sequelize.NOW
    }
}, {
    indexes: [] 
});



// Relations
// Scenario.belongsToMany(Menu, { through: 'ScenarioMenus' });
// Menu.belongsToMany(Scenario, { through: 'ScenarioMenus' });

Scenario.belongsToMany(Menu, { through: { model: ScenarioMenus, unique: false } });
Menu.belongsToMany(Scenario, { through: { model: ScenarioMenus, unique: false } });

Menu.belongsToMany(Cutlery, { through: MenuCutleries });
Cutlery.belongsToMany(Menu, { through: MenuCutleries });

Scenario.belongsToMany(Drink, { through: ScenarioDrinks, as: 'drinks' });
Drink.belongsToMany(Scenario, { through: ScenarioDrinks });

Scenario.belongsToMany(Extra, { through: 'ScenarioExtras' });
Extra.belongsToMany(Scenario, { through: 'ScenarioExtras' });

// Middleware
app.use(bodyParser.json());
app.use(express.static(path.join(__dirname, 'public')));
app.use('/images', express.static(path.join(__dirname, 'images')));


// ROUTES VIEWS

// Scenario
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'scenario-list.html'));
});

app.get('/views/scenario-create.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'scenario-create.html'));
});

app.get('/scenarios', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'scenario-list.html'));
});

app.get('/views/scenario-update.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'scenario-update.html'));
});

// Menus
app.get('/menus', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'menu-list.html'));
});

app.get('/views/menu-update.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'menu-update.html'));
});

app.get('/views/menu-create.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'menu-create.html'));
});

app.get('/help', (req, res) => {
    res.sendFile(path.join(__dirname, 'views', 'help.html'));
});


// ROUTES CRUD

// Get all scenarios
app.get('/api/scenarios', async (req, res) => {
    try {
        const scenarios = await Scenario.findAll({
            include: [
                { model: Menu, include: [Cutlery] },
                { model: Drink, as: 'drinks' },
                Extra
            ]
        });
        res.json(scenarios);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Route to get all scenarios formatted for Unity
app.get('/api/unityScenarios', async (req, res) => {
    try {
        const scenariosRaw = await sequelize.query(
            `SELECT 
                s.id AS scenarioId, 
                s.name AS scenarioName, 
                s.description AS scenarioDescription, 
                s.mode AS scenarioMode, 
                s.level AS scenarioLevel, 
                s.allowed_time AS allowedTime,
                s.validator_position AS validatorPosition, 
                s.validator_rotation AS validatorRotation, 
                s.validator_order AS validatorOrder,
                sm.id AS scenarioMenuId,
                m.id AS menuId, 
                m.name AS menuName, 
                m.description AS menuDescription, 
                m.appetizer AS menuAppetizer, 
                m.mainCourse AS menuMainCourse, 
                m.dessert AS menuDessert,
                c.id AS cutleryId,
                c.tag AS cutleryTag,
                c.text AS cutleryText,
                mc.parent AS cutleryParent,
                mc.placementOrderIndex AS cutleryPlacementOrderIndex,
                mc.offset AS cutleryOffset,
                d.id AS drinkId,
                d.type AS drinkType,
                sd.name AS drinkName,
                e.id AS extraId,
                e.name AS extraName,
                e.tag AS extraTag
            FROM Scenarios s
            LEFT JOIN ScenarioMenus sm ON sm.ScenarioId = s.id
            LEFT JOIN Menus m ON m.id = sm.MenuId
            LEFT JOIN MenuCutleries mc ON mc.MenuId = m.id
            LEFT JOIN Cutleries c ON c.id = mc.CutleryId
            LEFT JOIN ScenarioDrinks sd ON sd.ScenarioId = s.id
            LEFT JOIN Drinks d ON d.id = sd.DrinkId
            LEFT JOIN ScenarioExtras se ON se.ScenarioId = s.id
            LEFT JOIN Extras e ON e.id = se.ExtraId`,
            { type: Sequelize.QueryTypes.SELECT }
        );

        const formattedScenarios = scenariosRaw.reduce((acc, row) => {
            let scenario = acc.find(s => s.name === row.scenarioName);
            if (!scenario) {
                scenario = {
                    name: row.scenarioName,
                    description: row.scenarioDescription,
                    mode: row.scenarioMode,
                    level: row.scenarioLevel,
                    allowedTime: row.allowedTime,
                    validator: {
                        position: !!row.validatorPosition,
                        rotation: !!row.validatorRotation,
                        order: !!row.validatorOrder
                    },
                    menus: [],
                    drinks: [],
                    extras: []
                };
                acc.push(scenario);
            }
        
            if (row.drinkId && !scenario.drinks.some(drink => drink.type === row.drinkType)) {
                scenario.drinks.push({
                    type: row.drinkType,
                    name: row.drinkName
                });
            }
        
            let menu = scenario.menus.find(m => m.name === row.menuName);
            if (!menu) {
                menu = {
                    name: row.menuName,
                    description: row.menuDescription,
                    appetizer: row.menuAppetizer,
                    mainCourse: row.menuMainCourse,
                    dessert: row.menuDessert,
                    cutleryToPlace: {} 
                };
                scenario.menus.push(menu);
            }
        
            if (row.cutleryId) {
                if (!menu.cutleryToPlace) menu.cutleryToPlace = {};
                menu.cutleryToPlace[row.cutleryTag] = { 
                    tag: row.cutleryTag,
                    parent: row.cutleryParent,
                    placementOrderIndex: row.cutleryPlacementOrderIndex,
                    offset: row.cutleryOffset ? JSON.parse(row.cutleryOffset) : null
                };
            }

            if (row.extraId && !scenario.extras.some(extra => extra.name === row.extraName)) {
                scenario.extras.push({ name: row.extraName, tag : row.extraTag });
            }
        
            return acc;
        }, []);

        formattedScenarios.forEach(scenario => {
            scenario.menus.forEach(menu => {
                const sortedCutleryArray = Object.values(menu.cutleryToPlace).sort((a, b) => a.placementOrderIndex - b.placementOrderIndex);
                
                menu.cutleryToPlace = sortedCutleryArray.reduce((acc, cutlery) => {
                    acc[cutlery.tag] = cutlery;
                    return acc;
                }, {});
                
                delete menu.scenarioMenuId; 
            });
        });
        

        res.json(formattedScenarios);
    } catch (error) {
        console.error('Erreur lors de la récupération des scénarios Unity avec les boissons et les groupements appropriés:', error);
        res.status(500).json({ error: error.message });
    }
});

// Get scenario by ID
app.get('/api/scenarios/:id', async (req, res) => {
    try {
        const scenarioId = req.params.id;

        const scenarioMenus = await sequelize.query(`
            SELECT 
                sm.id AS scenarioMenuId, 
                sm.ScenarioId, 
                sm.MenuId, 
                m.id AS menuId, 
                m.name, 
                m.description, 
                m.appetizer, 
                m.mainCourse, 
                m.dessert, 
                m.createdAt AS menuCreatedAt, 
                m.updatedAt AS menuUpdatedAt 
            FROM 
                ScenarioMenus sm 
            JOIN 
                Menus m ON sm.MenuId = m.id 
            WHERE 
                sm.ScenarioId = :scenarioId
        `, {
            replacements: { scenarioId },
            type: sequelize.QueryTypes.SELECT
        });

        const scenario = await Scenario.findByPk(req.params.id, {
            include: [
                { model: Drink, as: 'drinks' },
                Extra
            ]
        });

        if (!scenario) {
            return res.status(404).json({ message: 'Scenario not found' });
        }

        scenario.setDataValue('menus', scenarioMenus);

        res.status(201).json(scenario);
    } catch (error) {
        console.error('Error fetching scenario:', error);
        res.status(500).json({ error: error.message });
    }
});

// Add scenario
app.post('/api/scenarios', async (req, res) => {
    const transaction = await sequelize.transaction();
    try {
        const data = req.body;

        // Create the scenario within a transaction
        const newScenario = await Scenario.create({
            name: data.name,
            description: data.description,
            mode: data.mode,
            level: data.level,
            allowed_time: data.allowedTime,
            validator_position: data.validator.position,
            validator_rotation: data.validator.rotation,
            validator_order: data.validator.order
        }, { transaction });

        // Add drinks and their bottle names to the scenario
        if (data.drinks) {
            for (const drinkData of data.drinks) {
                const [drink] = await Drink.findOrCreate({
                    where: { type: drinkData.type },
                    transaction
                });
                await newScenario.addDrink(drink, { through: { name: drinkData.name || null }, transaction });
            }
        }

        // Add menus (only by their IDs)
        if (data.menus) {
            for (const menuId of data.menus) {
                const menu = await Menu.findByPk(menuId, { transaction });
                if (menu) {
                    await ScenarioMenus.create({
                        ScenarioId: newScenario.id,
                        MenuId: menu.id
                    }, { transaction });
                }
            }
        }

        // Add extras to the scenario
        if (data.extras) {
            for (const extraData of data.extras) {
                const [extra] = await Extra.findOrCreate({
                    where: { name: extraData.name },
                    transaction
                });
                await newScenario.addExtra(extra, { transaction });
            }
        }

        // Commit transaction after all operations
        await transaction.commit();
        res.status(201).json({ message: 'Scenario added successfully' });
    } catch (error) {
        await transaction.rollback();
        console.error('Error creating scenario:', error);
        res.status(500).json({ error: error.message });
    }
});

// Update scenario
app.put('/api/scenarios/:id', async (req, res) => {
    const transaction = await sequelize.transaction();
    try {
        const scenarioId = req.params.id;
        const scenarioData = req.body;

        // Check if the scenario exists
        const scenario = await Scenario.findByPk(scenarioId, {
            include: [
                { model: Menu },
                { model: Drink, as: 'drinks' },
                Extra
            ]
        });

        if (!scenario) {
            await transaction.rollback();
            return res.status(404).json({ message: 'Scenario not found' });
        }

        // Update scenario data
        await scenario.update({
            name: scenarioData.name,
            description: scenarioData.description,
            mode: scenarioData.mode,
            level: scenarioData.level,
            allowed_time: scenarioData.allowedTime,
            validator_position: scenarioData.validator.position,
            validator_rotation: scenarioData.validator.rotation,
            validator_order: scenarioData.validator.order
        }, { transaction });

        const existingMenus = await scenario.getMenus({ transaction });
        const incomingMenuIds = scenarioData.menus.map(id => parseInt(id));
        console.log(existingMenus);
        console.log(incomingMenuIds); 

        for (const menu of existingMenus) {
            await scenario.removeMenu(menu, { transaction });
        }

        for (const menuId of incomingMenuIds) {
            const menu = await Menu.findByPk(menuId, { transaction });
            if (menu) {
                await ScenarioMenus.create({
                    ScenarioId: scenario.id,
                    MenuId: menu.id
                }, { transaction });
            }
        }

        const existingDrinks = await scenario.getDrinks({ transaction });
        const incomingDrinks = scenarioData.drinks || [];

        for (const drinkData of incomingDrinks) {
            const [drink] = await Drink.findOrCreate({
                where: { type: drinkData.type },
                transaction
            });

            const existingDrink = existingDrinks.find(d => d.type === drinkData.type);
            if (!existingDrink) {
                await scenario.addDrink(drink, { through: { name: drinkData.name || null }, transaction });
            }
        }

        for (const existingDrink of existingDrinks) {
            if (!incomingDrinks.find(d => d.type === existingDrink.type)) {
                await scenario.removeDrink(existingDrink, { transaction });
            }
        }

        const existingExtras = await scenario.getExtras({ transaction });
        const incomingExtras = scenarioData.extras || [];

        for (const extraData of incomingExtras) {
            const [extra] = await Extra.findOrCreate({
                where: { name: extraData.name },
                transaction
            });

            const existingExtra = existingExtras.find(e => e.name === extraData.name);
            if (!existingExtra) {
                await scenario.addExtra(extra, { transaction });
            }
        }

        for (const existingExtra of existingExtras) {
            if (!incomingExtras.find(e => e.name === existingExtra.name)) {
                await scenario.removeExtra(existingExtra, { transaction });
            }
        }

        // Commit transaction after all operations
        await transaction.commit();
        res.status(200).json({ message: 'Scenario updated successfully' });
    } catch (error) {
        await transaction.rollback();
        console.error('Error updating scenario:', error);
        res.status(500).json({ error: error.message });
    }
});

// Delete scenario by ID
app.delete('/api/scenarios/:id', async (req, res) => {
    try {
        const scenario = await Scenario.findByPk(req.params.id);
        if (!scenario) {
            return res.status(404).json({ message: 'Scenario not found' });
        }
        await scenario.destroy();
        res.json({ message: 'Scenario deleted successfully' });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Get all menus
app.get('/api/menus', async (req, res) => {
    try {
        const menus = await Menu.findAll({ include: [Cutlery] });
        res.json(menus);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Get a menu by ID
app.get('/api/menus/:id', async (req, res) => {
    try {
        const menu = await Menu.findByPk(req.params.id, { include: [Cutlery] });
        if (!menu) {
            return res.status(404).json({ message: 'Menu not found' });
        }
        res.json(menu);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Add a new menu
app.post('/api/menus', async (req, res) => {
    try {
        const { name, description, appetizer, mainCourse, dessert, cutleryToPlace } = req.body;

        const menu = await Menu.create({ name, description, appetizer, mainCourse, dessert });

        if (cutleryToPlace) {
            for (const [tag, cutleryData] of Object.entries(cutleryToPlace)) {
                const [cutlery] = await Cutlery.findOrCreate({
                    where: { tag: cutleryData.tag },
                    defaults: { text: cutleryData.text || '' }
                });
                await menu.addCutlery(cutlery, {
                    through: {
                        parent: cutleryData.parent,
                        placementOrderIndex: cutleryData.placementOrderIndex,
                        offset: cutleryData.offset
                    }
                });
            }
        }

        res.status(201).json({ message: 'Menu created successfully', menu });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Update a menu by ID
app.put('/api/menus/:id', async (req, res) => {
    try {
        const menuId = req.params.id;
        const { name, description, appetizer, mainCourse, dessert, cutleryToPlace } = req.body;

        const menu = await Menu.findByPk(menuId);
        if (!menu) {
            return res.status(404).json({ message: 'Menu not found' });
        }

        // Update menu details
        await menu.update({ name, description, appetizer, mainCourse, dessert });

        // Update cutleries
        const existingCutleries = await menu.getCutleries();
        for (const cutlery of existingCutleries) {
            await menu.removeCutlery(cutlery);
        }

        if (cutleryToPlace) {
            for (const [tag, cutleryData] of Object.entries(cutleryToPlace)) {
                const [cutlery] = await Cutlery.findOrCreate({
                    where: { tag: cutleryData.tag },
                    defaults: { text: cutleryData.text || '' }
                });
                await menu.addCutlery(cutlery, {
                    through: {
                        parent: cutleryData.parent,
                        placementOrderIndex: cutleryData.placementOrderIndex,
                        offset: cutleryData.offset
                    }
                });
            }
        }

        res.status(200).json({ message: 'Menu updated successfully', menu });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Delete a menu by ID
app.delete('/api/menus/:id', async (req, res) => {
    try {
        const menu = await Menu.findByPk(req.params.id);
        if (!menu) {
            return res.status(404).json({ message: 'Menu not found' });
        }
        await menu.destroy();
        res.json({ message: 'Menu deleted successfully' });
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Fetch all cutleries
app.get('/api/cutleries', async (req, res) => {
    try {
        const cutleries = await Cutlery.findAll();
        res.json(cutleries);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Fetch all drinks
app.get('/api/drinks', async (req, res) => {
    try {
        const drinks = await Drink.findAll();
        res.json(drinks);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Fetch all extras
app.get('/api/extras', async (req, res) => {
    try {
        const extras = await Extra.findAll();
        res.json(extras);
    } catch (error) {
        res.status(500).json({ error: error.message });
    }
});

// Initialize default data
async function initializeDefaultData() {
    const defaultDrinks = [
        { type: 'Eau minérale' },
        { type: 'Vin blanc' },
        { type: 'Vin rouge' }
    ];
    for (const drink of defaultDrinks) {
        await Drink.findOrCreate({ where: { type: drink.type } });
    }

    const defaultCutleries = [
        { tag: 'Plate', text: 'Assiette' },
        { tag: 'ButterKnife', text: 'Couteau à beurre' },
        { tag: 'CarvingFork', text: 'Fourchette à découper' },
        { tag: 'DessertFork', text: 'Fourchette à dessert' },
        { tag: 'DessertKnife', text: 'Couteau à dessert' },
        { tag: 'DessertSpoon', text: 'Cuillère à dessert' },
        { tag: 'DinnerFork', text: 'Fourchette à diner' },
        { tag: 'DinnerKnife', text: 'Couteau à diner' },
        { tag: 'DinnerSpoon', text: 'Cuillère à diner' },
        { tag: 'FishFork', text: 'Fourchette à poisson' },
        { tag: 'FishKnife', text: 'Couteau à poisson' },
        { tag: 'FruitFork', text: 'Fourchette à fruit' },
        { tag: 'FruitKnife', text: 'Couteau à fruit' },
        { tag: 'FruitSpoon', text: 'Cuillère à fruit' },
        { tag: 'GlassWater', text: 'Verre à eau' },
        { tag: 'GlassWine', text: 'Verre à vin' },
        { tag: 'SaladFork', text: 'Fourchette à salade' },
        { tag: 'SteakKnife', text: 'Couteau à viande' },
        { tag: 'TeaSpoon', text: 'Cuillère à thé' },
        { tag: 'Table', text: 'Table' }
    ];
    for (const cutlery of defaultCutleries) {
        await Cutlery.findOrCreate({ where: { tag: cutlery.tag }, defaults: cutlery });
    }

    // const defaultExtras = ['Pain', 'Carafe d\'eau', 'Vase de fleurs'];
    const defaultExtras = [
        { tag : 'Bread', name: 'Pain'},
        { tag : 'WaterJug', name: 'Carafe d\'eau'},
        { tag : 'FlowerVase', name: 'Vase de fleurs'}
    ];
    for (const extra of defaultExtras) {
        await Extra.findOrCreate({ where: { tag: extra.tag, name: extra.name, } });
    }
}

// Start server
sequelize.sync({ force: false }).then(async () => {
    await initializeDefaultData();
    app.listen(PORT, () => {
        console.log(`Server running at http://localhost:${PORT}`);
    });
}).catch(error => console.error('Database sync error:', error));
