const canvas = new fabric.Canvas('tableCanvas', { selection: true, backgroundColor: '#873b01' });

// Définition des ustensiles
const cutleryOptions = [
    { value: 'Plate', text: 'Assiette', scale : 0.75 },
    { value: 'ButterKnife', text: 'Couteau à beurre', scale : 0.2 },
    { value: 'CarvingFork', text: 'Fourchette à découper', scale : 0.4 },
    { value: 'DessertFork', text: 'Fourchette à dessert', scale : 0.225 },
    { value: 'DessertKnife', text: 'Couteau à dessert', scale : 0.275 },
    { value: 'DessertSpoon', text: 'Cuillère à dessert', scale : 0.225 },
    { value: 'DinnerFork', text: 'Fourchette à diner', scale : 0.3 },
    { value: 'DinnerKnife', text: 'Couteau à diner', scale : 0.4 },
    { value: 'DinnerSpoon', text: 'Cuillère à diner', scale : 0.3 },
    { value: 'FishFork', text: 'Fourchette à poisson', scale : 0.275 },
    { value: 'FishKnife', text: 'Couteau à poisson', scale : 0.275 },
    { value: 'FruitFork', text: 'Fourchette à fruit', scale : 0.2 },
    { value: 'FruitKnife', text: 'Couteau à fruit', scale : 0.25 },
    { value: 'FruitSpoon', text: 'Cuillère à fruit', scale : 0.175 },
    { value: 'GlassWater', text: 'Verre à eau', scale : 0.25 },
    { value: 'GlassWine', text: 'Verre à vin', scale : 0.25 },
    { value: 'SaladFork', text: 'Fourchette à salade', scale : 0.25 },
    { value: 'SteakKnife', text: 'Couteau à viande', scale : 0.4 },
    { value: 'TeaSpoon', text: 'Cuillère à thé', scale : 0.175 },
];

const utensils = cutleryOptions.map((cutlery, index) => ({
    id: index + 1,
    name: cutlery.value,
    icon: `/images/${cutlery.value}.png`,
    label: cutlery.text,
    scale : cutlery.scale
}));

// Initialisation des conteneurs
const iconContainer = document.getElementById('iconContainer');
const parentSelect = document.getElementById('parentSelect');
const centerSelectionPanel = document.getElementById('centerSelectionPanel');
const centerSelectionButton = document.getElementById('centerSelection');
const controlPanel = document.getElementById('controlPanel');

// Gestion du glisser-déposer
canvas.upperCanvasEl.addEventListener('dragover', (e) => e.preventDefault());

canvas.upperCanvasEl.addEventListener('drop', (e) => {
    e.preventDefault();

    const name = e.dataTransfer.getData('name');
    let icon = `/images/${name}.png`
    if (name === 'GlassWine' || name === 'GlassWater') {
        icon = `/images/${name}_top.png`
    } else if (name !== 'Plate') {
        icon = `/images/${name}_reversed.png`
    }

    if (canvas.getObjects().some((obj) => obj.data?.name === name)) {
        alert(`L'ustensile "${name}" est déjà ajouté.`);
        return;
    }

    const rect = canvas.upperCanvasEl.getBoundingClientRect();
    const pointer = { x: e.clientX - rect.left, y: e.clientY - rect.top };
    const scale = cutleryOptions.find(item => item.value === name).scale;

    fabric.Image.fromURL(icon, (img) => {
        if (!img) return;
        img.set({
            left: pointer.x, 
            top: pointer.y,  
            originX: 'center', 
            originY: 'center', 
            scaleX: scale,
            scaleY: scale,
            hasControls: false,
            hasBorders: true,
            selectable: true,
            centeredRotation: true,
            data: { name, parent: 'Table', placementOrderIndex: 0 },
        });
        canvas.add(img);
        img.on('mousedown', () => showControlPanel(img));
        canvas.renderAll();

        const option = document.createElement('option');
        option.value = name;
        option.textContent = name;
        parentSelect.appendChild(option);
    });
});

// Fonction pour afficher le panneau de contrôle
function showControlPanel(object) {
    controlPanel.classList.remove('hidden');

    const posXInput = document.getElementById('posX');
    const posYInput = document.getElementById('posY');
    const rotationInput = document.getElementById('rotation');
    const placementOrderInput = document.getElementById('placementOrder');
    const buttonDelete = document.getElementById('deleteObject'); // delete selected object from the canvas

    const centerX = object.left; 
    const centerY = object.top; 

    posXInput.value = Math.round(centerX || 0);
    posYInput.value = Math.round(centerY || 0);
    rotationInput.value = Math.round(object.angle || 0);
    placementOrderInput.value = object.data.placementOrderIndex || 0;

    updateParentSelect(object);

    posXInput.oninput = () => {
        const newX = parseInt(posXInput.value, 10);
        object.set({
            left: newX 
        });
        canvas.renderAll();
    };

    posYInput.oninput = () => {
        const newY = parseInt(posYInput.value, 10);
        object.set({
            top: newY 
        });
        canvas.renderAll();
    };

    rotationInput.oninput = () => {
        object.set({ angle: parseInt(rotationInput.value, 10) });
        canvas.renderAll();
    };

    placementOrderInput.oninput = () => {
        object.data.placementOrderIndex = parseInt(placementOrderInput.value, 10) || 0;
        canvas.renderAll();
    };

    parentSelect.onchange = () => {
        object.data.parent = parentSelect.value;
    };

    object.on('moving', () => {
        posXInput.value = Math.round(object.left || 0); 
        posYInput.value = Math.round(object.top || 0);  
    });

    object.on('rotating', () => {
        rotationInput.value = Math.round(object.angle || 0);
    });

    buttonDelete.onclick = () => {
        canvas.remove(object); 
        controlPanel.classList.add('hidden'); 
        canvas.renderAll(); 
    };
}

// Masquer le panneau de contrôle lorsqu'on clique à l'extérieur
canvas.on('mouse:down', (event) => {
    if (!event.target) {
        controlPanel.classList.add('hidden');
    }
});

// Mise à jour de la liste des parents
function updateParentSelect(currentObject = null) {
    parentSelect.innerHTML = '<option value="Table">Table</option>';
    canvas.getObjects()
        .filter((obj) => obj.data?.name && obj !== currentObject)
        .forEach((obj) => {
            const option = document.createElement('option');
            option.value = obj.data.name;
            option.textContent = obj.data.name;
            parentSelect.appendChild(option);
        });

    if (currentObject && currentObject.data.parent) {
        parentSelect.value = currentObject.data.parent;
    }
}

// Affichage du bouton "Centrer" pour plusieurs objets sélectionnés
canvas.on('selection:created', () => {
    const activeSelection = canvas.getActiveObject();
    if (activeSelection && activeSelection.type === 'activeSelection' && Array.isArray(activeSelection._objects) && activeSelection._objects.length > 1) {
        centerSelectionPanel.style.display = 'block'; // Affiche le bouton si plusieurs objets sont sélectionnés
    } else {
        centerSelectionPanel.style.display = 'none'; // Cache le bouton pour une sélection unique ou aucun objet
    }
});

canvas.on('selection:updated', () => {
    const activeSelection = canvas.getActiveObject();
    if (activeSelection && activeSelection.type === 'activeSelection' && Array.isArray(activeSelection._objects) && activeSelection._objects.length > 1) {
        centerSelectionPanel.style.display = 'block'; // Affiche le bouton si plusieurs objets sont sélectionnés
    } else {
        centerSelectionPanel.style.display = 'none'; // Cache le bouton pour une sélection unique ou aucun objet
    }
});


canvas.on('selection:cleared', () => {
    centerSelectionPanel.style.display = 'none'; // Cache le bouton lorsqu'aucun objet n'est sélectionné
});

// Fonction pour centrer la sélection multiple
centerSelectionButton.addEventListener('click', () => {
    const activeSelection = canvas.getActiveObject();
    if (activeSelection && activeSelection.type === 'activeSelection') {
        activeSelection.center();
        activeSelection.setCoords();
        canvas.renderAll();
    }
});

// Vérification des duplications de placementOrderIndex
function hasDuplicatePlacementOrderIndex() {
    const indexes = canvas.getObjects()
        .filter((obj) => obj.data && obj.data.placementOrderIndex !== undefined)
        .map((obj) => obj.data.placementOrderIndex);

    return indexes.length !== new Set(indexes).size;
}

// Ajout des icônes des ustensiles
function populateCutleryList() {
    utensils.forEach((utensil) => {
        const container = document.createElement('div');
        container.classList.add('flex', 'flex-col', 'items-center');

        const div = document.createElement('div');
        div.classList.add('w-16', 'h-16', 'bg-gray-200', 'border', 'rounded', 'flex', 'items-center', 'justify-center', 'cursor-pointer', 'hover:bg-gray-300', 'utensil-icon');
        div.style.backgroundImage = `url('${utensil.icon}')`;
        div.setAttribute('data-id', utensil.id);
        div.setAttribute('data-name', utensil.name);
        div.setAttribute('data-icon', utensil.icon);
        div.draggable = true;

        div.ondragstart = (e) => {
            e.dataTransfer.setData('name', utensil.name);
            e.dataTransfer.setData('icon', utensil.icon);
        };

        const label = document.createElement('span');
        label.classList.add('block', 'text-center', 'text-xs', 'mt-1', 'font-semibold');
        label.textContent = utensil.label;

        container.appendChild(div);
        container.appendChild(label);
        iconContainer.appendChild(container);
    });
}

// Remplit le canevas et les champs avec les données d'un menu
function populateMenu(menuData) {
    if (!menuData || !menuData.Cutleries) {
        console.error('Données de menu invalides ou manquantes');
        alert('Erreur lors de la récupération des données du menu.');
        return;
    }

    // Remplit les champs du formulaire
    document.querySelector('.menu-name').value = menuData.name || '';
    document.querySelector('.menu-description').value = menuData.description || '';
    document.querySelector('.menu-appetizer').value = menuData.appetizer || '';
    document.querySelector('.menu-main-course').value = menuData.mainCourse || '';
    document.querySelector('.menu-dessert').value = menuData.dessert || '';

    const canvasCenter = { x: canvas.getWidth() / 2, y: canvas.getHeight() / 2 };
    const objectsToAdjust = [];
    menuData.Cutleries.forEach((cutlery) => {
        const offset = cutlery.MenuCutleries?.offset || null;
        const tag = cutlery.tag;
        let icon = `/images/${tag}.png`
        if (tag === 'GlassWine' || tag === 'GlassWater') {
            icon = `/images/${tag}_top.png`
        } else if (tag !== 'Plate') {
            icon = `/images/${tag}_reversed.png`
        }

        if (!offset || !tag) {
            console.warn(`Données manquantes pour l'ustensile : ${cutlery}`);
            return;
        }

        const scale = cutleryOptions.find(item => item.value === tag).scale;


        fabric.Image.fromURL(icon, (img) => {
            if (!img) return;

            img.set({
                left: canvasCenter.x,
                top: canvasCenter.y,
                originX: 'center', 
                originY: 'center', 
                scaleX: scale,
                scaleY: scale,
                angle: 0,
                hasControls: false,
                hasBorders: true,
                selectable: true,
                centeredRotation: true,
                data: {
                    name: tag,
                    parent: cutlery.MenuCutleries.parent || 'Table',
                    placementOrderIndex: cutlery.MenuCutleries.placementOrderIndex || 0,
                    offset: cutlery.MenuCutleries.offset,
                },
            });

            canvas.add(img);
            objectsToAdjust.push(img);
            img.on('mousedown', () => showControlPanel(img));
            canvas.renderAll();
        });
    });

    setTimeout(() => {
        objectsToAdjust.forEach((obj) => {
            const { offset, parent } = obj.data;

            const parentObj = parent === 'Table'
                ? null
                : canvas.getObjects().find((o) => o.data?.name === parent);

            const absolutePosition = parentObj
            ? calculateAbsolutePosition(offset.position, parentObj, canvasCenter)
            : calculateAbsolutePosition(offset.position, null, canvasCenter);

            obj.set({
                left: absolutePosition.left,
                top: absolutePosition.top,
                angle: Math.atan2(offset.rotation.z, offset.rotation.w) * (180 / Math.PI) * 2,
            });

            obj.setCoords(); 
        });

        canvas.renderAll(); 
    }, 100);
}

// Initialisation
async function initializePage() {
    populateCutleryList();

    const menuId = new URLSearchParams(window.location.search).get('id');
    if (menuId) {
        const menuData = await fetchMenuData(menuId);
        if (menuData) {
            populateMenu(menuData);
        }
    } else {
        alert('Aucun ID de menu trouvé.');
    }
}

// Fetch menu data by ID
async function fetchMenuData(menuId) {
    try {
        const response = await fetch(`/api/menus/${menuId}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch menu: ${response.statusText}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching menu:', error);
        alert('Erreur lors de la récupération du menu.');
        return null;
    }
}

document.getElementById('save-menu').addEventListener('click', async () => {
    if (hasDuplicatePlacementOrderIndex()) {
        alert('Des doublons ont été détectés dans les index d’ordre. Veuillez corriger avant de sauvegarder.');
        return;
    }

    const menuName = document.querySelector('.menu-name').value.trim();
    const menuDescription = document.querySelector('.menu-description').value.trim();
    const menuAppetizer = document.querySelector('.menu-appetizer').value.trim();
    const menuMainCourse = document.querySelector('.menu-main-course').value.trim();
    const menuDessert = document.querySelector('.menu-dessert').value.trim();

    if (!menuName || !menuDescription || !menuAppetizer || !menuMainCourse || !menuDessert) {
        alert('Veuillez remplir tous les champs du menu.');
        return;
    }

    const canvasCenter = { x: canvas.getWidth() / 2, y: canvas.getHeight() / 2 };

    const cutleries = canvas.getObjects()
        .filter((obj) => obj.data && obj.data.name)
        .map((obj) => {
            let parentObj = canvas.getObjects().filter((o) => o.data.name == obj.data.parent);
            if (parentObj != undefined)
                parentObj = parentObj[0];
            const position = calculateRelativePosition(obj, parentObj, canvasCenter);
            const rotation = convertCanvasRotationToQuaternion(obj.angle || 0, obj.data.name);
            console.log(rotation);

            return {
                tag: obj.data.name,
                parent: obj.data.parent || 'Table',
                placementOrderIndex: obj.data.placementOrderIndex || 0,
                offset: { position, rotation },
            };
        });

    const menuData = {
        name: menuName,
        description: menuDescription,
        appetizer: menuAppetizer,
        mainCourse: menuMainCourse,
        dessert: menuDessert,
        cutleryToPlace: cutleries.reduce((acc, cur) => {
            acc[cur.tag] = cur;
            return acc;
        }, {}),
    };

    try {
        const menuId = new URLSearchParams(window.location.search).get('id');
        const response = await fetch(`/api/menus/${menuId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(menuData),
        });

        if (response.ok) {
            alert('Menu sauvegardé avec succès!');
        } else {
            alert('Erreur lors de la sauvegarde du menu.');
        }
    } catch (error) {
        alert('Une erreur est survenue lors de la sauvegarde.');
        console.error(error);
    }
});

function calculateRelativePosition(obj, parentObj, canvasCenter) {
    const divider = 1000; 

    const objectCenterX = obj.left; 
    const objectCenterY = obj.top;

    if (!parentObj || parentObj.data.parent === "Table") {
        return {
            x: (objectCenterX - canvasCenter.x) / divider,
            y: 0.81, 
            z: -(objectCenterY - canvasCenter.y) / divider,
        };
    }

    const parentCenterX = parentObj.left;
    const parentCenterY = parentObj.top;

    return {
        x: (objectCenterX - parentCenterX) / divider,
        y: 0,
        z: -(objectCenterY - parentCenterY) / divider,
    };
}


function convertCanvasRotationToQuaternion(angleInDegrees, objName) {
    if (objName === "GlassWater" || objName === "GlassWine") {
        return {
            x: 1,
            y: 0,
            z: 0,
            w: 6.123233995736766e-17
        };
    } else if(objName === "Plate") {
        return {
            x: -0.7071067811865477,
            y: 0,
            z: 0,
            w: 0.7071067811865477
        };
    } else {
        const radians = (angleInDegrees * Math.PI) / 180;
        return {
            x: 0,
            y: 0,
            z: Math.sin(radians / 2),
            w: Math.cos(radians / 2)
        };
    }
}

function calculateAbsolutePosition(relativePosition, parentObj, canvasCenter) {
    const divider = 1000; 

    if (!parentObj || parentObj.data.parent === "Table") {
        return {
            left: (relativePosition.x * divider) + canvasCenter.x,
            top: canvasCenter.y - (relativePosition.z * divider),
        };
    }

    const parentCenterX = parentObj.left; 
    const parentCenterY = parentObj.top;

    return {
        left: (relativePosition.x * divider) + parentCenterX,
        top: parentCenterY - (relativePosition.z * divider),
    };
}


// function calculateAbsolutePosition(relativePosition, parentObj, canvasCenter) {
//     const divider = 1000;

//     if (!parentObj || parentObj.parent === "Table") {
//         return {
//             left: (relativePosition.x * divider) + canvasCenter.x,
//             top: canvasCenter.y - (relativePosition.z * divider),
//         };
//     }

//     const parentCenterX = parentObj.left + (parentObj.width * parentObj.scaleX) / 2;
//     const parentCenterY = parentObj.top + (parentObj.height * parentObj.scaleY) / 2;

//     return {
//         left: (relativePosition.x * divider) + parentCenterX - (parentObj.width * parentObj.scaleX) / 2,
//         top: parentCenterY - (relativePosition.z * divider) - (parentObj.height * parentObj.scaleY) / 2,
//     };
// }


// Vérification des duplications de placementOrderIndex
function hasDuplicatePlacementOrderIndex() {
    const indexes = canvas.getObjects()
        .filter((obj) => obj.data && obj.data.placementOrderIndex !== undefined)
        .map((obj) => obj.data.placementOrderIndex);

    return indexes.length !== new Set(indexes).size;
}

initializePage();