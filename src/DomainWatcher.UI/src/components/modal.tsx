import { Component } from "solid-js";
import { JSX } from "solid-js/jsx-runtime";
import { Portal, Show } from "solid-js/web";

export interface ModalProps {
  show: boolean;
  close: () => void;
  title: string;
  children: JSX.Element;
  class?: string;
}

export const Modal: Component<ModalProps> = (props) => {
  return <Show when={props.show}>
    <Portal>
      <div class="modal-wrapper">
        <div class={'modal ' + props.class}>
          <div class="title">
            <div class="title-text">
              { props.title }
            </div>
            <button onClick={_ => props.close()}>Close</button>
          </div>
          <div class="content">
            { props.children }
          </div>
        </div>
      </div>
    </Portal>
  </Show>;
}
