document.addEventListener('DOMContentLoaded', function() {
    fetch('/api/scenarios')
        .then(response => response.json())
        .then(scenarios => {
            const scenariosList = document.getElementById('scenarios-list');
            scenarios.forEach(scenario => {
                const scenarioCard = document.createElement('div');
                scenarioCard.className = 'p-4 border rounded bg-gray-100 flex justify-between items-center';

                const scenarioInfo = document.createElement('div');
                scenarioInfo.innerHTML = `
                    <h2 class="text-xl font-bold">${scenario.name}</h2>
                    <p class="text-gray-700">${scenario.description}</p>
                `;

                const buttonsContainer = document.createElement('div');
                buttonsContainer.className = 'space-x-4';

                const editButton = document.createElement('button');
                editButton.className = 'edit-scenario text-white px-4 py-2 rounded';
                editButton.textContent = 'Modifier';
                editButton.addEventListener('click', function() {
                    window.location.href = `/views/scenario-update.html?id=${scenario.id}`;
                });

                const deleteButton = document.createElement('button');
                deleteButton.className = 'delete-scenario text-white px-4 py-2 rounded';
                deleteButton.textContent = 'Supprimer';
                deleteButton.addEventListener('click', function() {
                    if (confirm('Êtes-vous sûr de vouloir supprimer ce scénario ?')) {
                        fetch(`/api/scenarios/${scenario.id}`, {
                            method: 'DELETE'
                        })
                        .then(response => {
                            if (response.ok) {
                                alert('Scénario supprimé avec succès');
                                scenarioCard.remove();
                            } else {
                                alert("Une erreur s'est produite lors de la suppression du scénario.");
                            }
                        })
                        .catch(error => {
                            console.error('Erreur:', error);
                            alert("Une erreur s'est produite lors de la suppression du scénario.");
                        });
                    }
                });

                buttonsContainer.appendChild(editButton);
                buttonsContainer.appendChild(deleteButton);

                scenarioCard.appendChild(scenarioInfo);
                scenarioCard.appendChild(buttonsContainer);

                scenariosList.appendChild(scenarioCard);
            });
        })
        .catch(error => {
            console.error('Erreur:', error);
            alert("Une erreur s'est produite lors du chargement des scénarios.");
        });
});
