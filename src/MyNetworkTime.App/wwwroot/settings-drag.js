window.myNetworkTimeSettings = (() => {
    let dotNetReference = null;
    let moveHandler = null;
    let upHandler = null;
    let cancelHandler = null;
    let currentHoverIndex = null;

    function getServerIndexFromPoint(clientX, clientY) {
        const target = document.elementFromPoint(clientX, clientY);
        const row = target?.closest?.('[data-server-index]');
        if (!row) {
            return null;
        }

        const index = Number.parseInt(row.dataset.serverIndex, 10);
        return Number.isNaN(index) ? null : index;
    }

    function cleanupHandlers() {
        if (moveHandler) {
            document.removeEventListener('pointermove', moveHandler, true);
            moveHandler = null;
        }

        if (upHandler) {
            document.removeEventListener('pointerup', upHandler, true);
            upHandler = null;
        }

        if (cancelHandler) {
            document.removeEventListener('pointercancel', cancelHandler, true);
            cancelHandler = null;
        }

        currentHoverIndex = null;
    }

    return {
        initialize(reference) {
            dotNetReference = reference;
        },

        dispose() {
            cleanupHandlers();
            dotNetReference = null;
        },

        beginPointerDrag(event, handle) {
            if (!dotNetReference || !handle) {
                return;
            }

            const row = handle.closest?.('[data-server-index]');
            if (!row) {
                return;
            }

            const sourceIndex = Number.parseInt(row.dataset.serverIndex, 10);
            if (Number.isNaN(sourceIndex)) {
                return;
            }

            dotNetReference.invokeMethodAsync('StartServerDragFromJsAsync', sourceIndex);

            moveHandler = moveEvent => {
                const hoverIndex = getServerIndexFromPoint(moveEvent.clientX, moveEvent.clientY);
                if (hoverIndex === null || hoverIndex === currentHoverIndex) {
                    return;
                }

                currentHoverIndex = hoverIndex;
                dotNetReference.invokeMethodAsync('SetDropTargetFromJsAsync', hoverIndex);
            };

            upHandler = upEvent => {
                const dropIndex = getServerIndexFromPoint(upEvent.clientX, upEvent.clientY);
                cleanupHandlers();

                if (dropIndex === null) {
                    dotNetReference.invokeMethodAsync('ClearServerDragFromJsAsync');
                    return;
                }

                dotNetReference.invokeMethodAsync('CompleteServerDropFromJsAsync', dropIndex);
            };

            cancelHandler = () => {
                cleanupHandlers();
                dotNetReference.invokeMethodAsync('ClearServerDragFromJsAsync');
            };

            document.addEventListener('pointermove', moveHandler, true);
            document.addEventListener('pointerup', upHandler, true);
            document.addEventListener('pointercancel', cancelHandler, true);

            event.preventDefault();
        }
    };
})();
